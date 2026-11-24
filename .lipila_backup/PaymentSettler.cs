using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Modules.Endpoints;

namespace MiniLipila.Modules;

public class SettleResult
{
    public PaymentStatus NewStatus { get; set; }
    public string LipilaStatus { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool WalletChanged { get; set; }
}

public class PaymentSettler(AppDbContext db, IConfiguration config)
{
    // Asks Lipila about one Pending payment and settles it.
    // Throws FlurlHttpException if Lipila cannot be reached or refuses the check.
    public async Task<SettleResult> SettleAsync(Payment payment, CancellationToken ct)
    {
        var apiKey = config["Lipila:ApiKey"]
            ?? throw new InvalidOperationException("Lipila:ApiKey is not set");
        var baseUrl = (config["Lipila:BaseUrl"]
                       ?? throw new InvalidOperationException("Lipila:BaseUrl is not set")).TrimEnd('/');
        
        var isCollection = payment.Type == PaymentType.Collection;
        var path = isCollection ? "collections" : "disbursements";

        var result = await $"{baseUrl}/api/v1/{path}/check-status"
            .SetQueryParam("referenceId", payment.Id.ToString())
            .WithHeader("x-api-key", apiKey)
            .GetJsonAsync<LipilaPaymentResponse>();

        var newStatus = result.Status switch
        {
            "Successful" => PaymentStatus.Successful,
            "Failed" => PaymentStatus.Failed,
            _ => PaymentStatus.Pending
        };

        var walletChanged = false;

        if (newStatus != PaymentStatus.Pending)
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // Only one caller can move the payment away from Pending.
            var changed = await db.Payments
                .Where(p => p.Id == payment.Id && p.Status == PaymentStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, newStatus)
                    .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow), ct);

            // money in: add it when it succeeded. money out: give it back when it failed.
            var moveMoney = (isCollection && newStatus == PaymentStatus.Successful)
                         || (!isCollection && newStatus == PaymentStatus.Failed);

            if (changed == 1 && moveMoney)
            {
                await db.Wallets
                    .Where(w => w.Id == payment.WalletId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(w => w.Balance, w => w.Balance + payment.Amount), ct);
                walletChanged = true;
            }

            await tx.CommitAsync(ct);
        }

        return new SettleResult
        {
            NewStatus = newStatus,
            LipilaStatus = result.Status,
            Message = result.Message,
            WalletChanged = walletChanged
        };
    }
}