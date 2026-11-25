using FastEndpoints;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Lipila;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;
using MiniLipila.Services;

namespace MiniLipila.Modules.Endpoints;

public class InitiateDisbursementEndpoint(
    AppDbContext db,
    IConfiguration config,
    IOptions<LipilaSettings> lipila,
    CustomerResolver resolver)
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
        Roles(MiniLipila.Models.Roles.Merchant, MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(DisbursementReq req, CancellationToken ct)
    {
        // 0. a merchant user may only pay out from their own merchant, checked before anything else
        if (!User.CanAccess(req.MerchantId))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        // 1. the request was already checked by DisbursementReqValidator
        var phone = Phone.ToLipilaFormat(req.AccountNumber)!;
        var narration = req.Narration.Trim();

        var apiKey = config["Lipila:ApiKey"]
                     ?? throw new InvalidOperationException("Lipila:ApiKey is not set");

        // 2. the wallet must exist AND belong to this merchant (this also proves the merchant exists)
        var walletExists = await db.Wallets
            .AnyAsync(w => w.Id == req.WalletId && w.MerchantId == req.MerchantId, ct);
        if (!walletExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 3. find or create the customer, and link this merchant to them.
        //    This runs BEFORE any money moves, so a bad number costs nothing.
        var resolved = await resolver.ResolveAsync(req.MerchantId, phone, ct);
        if (resolved.Customer is null)
        {
            if (resolved.Status == KycStatus.NotFound)
            {
                AddError("This number is not registered on mobile money.");
                await Send.ErrorsAsync(400, ct);
            }
            else
            {
                AddError("We could not verify this number right now. Please try again.");
                await Send.ErrorsAsync(502, ct);
            }
            return;
        }

        // 4. take the money out and save the payment as Pending, together or not at all
        var payment = new Payment
        {
            MerchantId = req.MerchantId,
            WalletId = req.WalletId,
            CustomerId = resolved.Customer.Id,
            Amount = req.Amount,
            AccountNumber = phone,
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

        // 5. send it to Lipila
        LipilaPaymentResponse result;
        try
        {
            result = await $"{lipila.Value.BaseUrl}{lipila.Value.DisbursementPath}"
                .WithHeader("x-api-key", apiKey)
                .WithHeader("callbackUrl", lipila.Value.CallbackUrl)
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

        // 6. reply. The status stays Pending until we check it with Lipila.
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
