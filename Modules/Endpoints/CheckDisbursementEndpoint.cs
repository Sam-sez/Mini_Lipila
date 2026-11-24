using FastEndpoints;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Lipila;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class CheckDisbursementEndpoint(AppDbContext db, IConfiguration config)
    : EndpointWithoutRequest<DisbursementStatusResponse>
{
    public override void Configure()
    {
        Get("/disbursements/{id}/status");
        Summary(s =>
        {
            s.Description = "`this asks Lipila what happened to a payout, updates it, and gives the money back to the wallet once if it failed`";
            s.Summary = "Check a disbursement's status";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var payment = await db.Payments.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.Type == PaymentType.Disbursement, ct);
        if (payment is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Already settled: do not ask again, and never touch the wallet again.
        if (payment.Status != PaymentStatus.Pending)
        {
            await Send.OkAsync(new DisbursementStatusResponse
            {
                PaymentId = payment.Id,
                OurStatus = payment.Status.ToString(),
                LipilaStatus = "not checked, already settled",
                WalletRefunded = false,
                Message = "This payout is already settled."
            }, ct);
            return;
        }

        var apiKey = config["Lipila:ApiKey"]
            ?? throw new InvalidOperationException("Lipila:ApiKey is not set");

        LipilaPaymentResponse result;
        try
        {
            result = await "https://api.lipila.dev/api/v1/disbursements/check-status"
                .SetQueryParam("referenceId", payment.Id.ToString())
                .WithHeader("x-api-key", apiKey)
                .GetJsonAsync<LipilaPaymentResponse>();
        }
        catch (FlurlHttpException ex)
        {
            AddError($"Lipila could not check the payout (status {ex.StatusCode}): {await ex.GetResponseStringAsync()}");
            await Send.ErrorsAsync(502, ct);
            return;
        }

        var newStatus = result.Status switch
        {
            "Successful" => PaymentStatus.Successful,
            "Failed" => PaymentStatus.Failed,
            _ => PaymentStatus.Pending
        };

        var refunded = false;

        if (newStatus != PaymentStatus.Pending)
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // Only one caller can move the payment away from Pending.
            var changed = await db.Payments
                .Where(p => p.Id == payment.Id && p.Status == PaymentStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, newStatus)
                    .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow), ct);

            // A failed payout gives the money back, once.
            if (changed == 1 && newStatus == PaymentStatus.Failed)
            {
                await db.Wallets
                    .Where(w => w.Id == payment.WalletId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(w => w.Balance, w => w.Balance + payment.Amount), ct);
                refunded = true;
            }

            await tx.CommitAsync(ct);
        }

        await Send.OkAsync(new DisbursementStatusResponse
        {
            PaymentId = payment.Id,
            OurStatus = newStatus.ToString(),
            LipilaStatus = result.Status,
            WalletRefunded = refunded,
            Message = result.Message
        }, ct);
    }
}
