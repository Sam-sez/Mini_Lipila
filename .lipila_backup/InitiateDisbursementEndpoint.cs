using FastEndpoints;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;

namespace MiniLipila.Modules.Endpoints;

public class DisbursementReq
{
    public Guid MerchantId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}

public class DisbursementResponse
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Created { get; set; } = string.Empty;
}

public class LipilaDisbursementBody
{
    public string ReferenceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}

public class InitiateDisbursementEndpoint(AppDbContext db, IConfiguration config)
    : Endpoint<DisbursementReq, DisbursementResponse>
{
    public override void Configure()
    {
        Post("/disbursements/mobile-money");
        Summary(s =>
        {
            s.Description = "`this takes money out of a merchant's wallet and sends it to any mobile money number through Lipila`";
            s.Summary = "Start a disbursement";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(DisbursementReq req, CancellationToken ct)
    {
        // 1. check the request itself
        if (req.Amount <= 0)
            AddError(r => r.Amount, "Amount must be greater than 0");

        var phone = Phone.ToLipilaFormat(req.AccountNumber);
        if (phone is null)
            AddError(r => r.AccountNumber, "Enter a Zambian number like 0979086825 or 260979086825");

        var narration = req.Narration.Trim();
        if (narration.Length is < 3 or > 200)
            AddError(r => r.Narration, "Narration must be 3 to 200 characters");

        ThrowIfAnyErrors();

        var apiKey = config["Lipila:ApiKey"]// all config to move to app settings
                     ?? throw new InvalidOperationException("Lipila:ApiKey is not set");
        var baseUrl = (config["Lipila:BaseUrl"]
                       ?? throw new InvalidOperationException("Lipila:BaseUrl is not set")).TrimEnd('/');
        var callbackUrl = config["Lipila:CallbackUrl"]
                          ?? throw new InvalidOperationException("Lipila:CallbackUrl is not set");

        // 2. the wallet must exist AND belong to this merchant
        var walletExists = await db.Wallets
            .AnyAsync(w => w.Id == req.WalletId && w.MerchantId == req.MerchantId, ct);
        if (!walletExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 3. take the money out and save the payment as Pending, together or not at all
        var payment = new Payment
        {
            MerchantId = req.MerchantId,
            WalletId = req.WalletId,
            Amount = req.Amount,
            AccountNumber = phone!,
            Narration = narration,
            Status = PaymentStatus.Pending,
            Type = PaymentType.Disbursement
        };

        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            // only works if the wallet has enough money
            var taken = await db.Wallets
                .Where(w => w.Id == req.WalletId && w.Balance >= req.Amount)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(w => w.Balance, w => w.Balance - req.Amount), ct);

            if (taken == 0)
            {
                AddError("Not enough money in this wallet");
                await Send.ErrorsAsync(400, ct);
                return;
            }

            await db.Payments.AddAsync(payment, ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
//lets create a seperate thi who's main purpose is just disbursements and seenwhether it works
        // 4. send it to Lipila
        LipilaPaymentResponse result;
        try
        {
            //todo: move the url to appsettings
            result = await $"{baseUrl}/api/v1/disbursements/mobile-money"
                .WithHeader("x-api-key", apiKey)
                .WithHeader("callbackUrl", callbackUrl)
                .WithHeader("accept", "application/json")
                .PostJsonAsync(new
                {
                    referenceId = payment.Id.ToString(),
                    amount = payment.Amount,
                    narration = payment.Narration,
                    accountNumber = payment.AccountNumber,
                    currency = "ZMW",
                    referenceData = "string"
                })
                .ReceiveJson<LipilaPaymentResponse>();
        }
        catch (FlurlHttpException ex) when (ex.StatusCode is >= 400)
        {
            // Lipila clearly refused it: mark Failed and give the money back, once
            var reason = await ex.GetResponseStringAsync();
           

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            var changed = await db.Payments
                .Where(p => p.Id == payment.Id && p.Status == PaymentStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, PaymentStatus.Failed)
                    .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow), ct);

            if (changed == 1)
            {
                await db.Wallets
                    .Where(w => w.Id == payment.WalletId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(w => w.Balance, w => w.Balance + payment.Amount), ct);
            }

            await tx.CommitAsync(ct);

            AddError($"Lipila rejected the request (status {ex.StatusCode}): {reason}");
            await Send.ErrorsAsync(502, ct);
            return;
        }
        catch (FlurlHttpException)
        {
            // no clear answer: we do not know what happened, so leave it Pending
            AddError("Lipila did not give a clear answer. The payout is left Pending and the money stays held until it is checked.");
            await Send.ErrorsAsync(502, ct);
            return;
        }

        // 5. reply. The status stays Pending until we check it with Lipila.
        await Send.ResponseAsync(new DisbursementResponse
        {
            PaymentId = payment.Id,
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Message = string.IsNullOrWhiteSpace(result.Message)
                ? "Payout request sent."
                : result.Message,
            Created = payment.CreatedAt.ToOffset(TimeSpan.FromHours(2)).ToString("dd MMM yyyy, HH:mm")
        }, 201, ct);
    }
}