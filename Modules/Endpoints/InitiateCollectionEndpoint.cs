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

namespace MiniLipila.Modules.Endpoints;

public class InitiateCollectionEndpoint(AppDbContext db, IConfiguration config, IOptions<LipilaSettings> lipila)
    : Endpoint<CollectionReq, CollectionResponse>
{
    public override void Configure()
    {
        Post("/collection");
        Summary(s =>
        {
            s.Description = "`this asks Lipila to collect money from a customer's mobile money into a merchant's wallet`";
            s.Summary = "Start a collection";
        });
        Roles(MiniLipila.Models.Roles.Merchant, MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(CollectionReq req, CancellationToken ct)
    {
        var apiKey = config["Lipila:ApiKey"]
                     ?? throw new InvalidOperationException("Lipila:ApiKey is not set");

        // 1. the wallet must exist AND belong to this merchant (this also proves the merchant exists)
        var walletExists = await db.Wallets
            .AnyAsync(w => w.Id == req.WalletId && w.MerchantId == req.MerchantId, ct);
        if (!walletExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 2. look the customer up ONCE, and only if one was chosen
        Customer? customer = null;
        if (req.CustomerId is not null)
        {
            customer = await db.Customers
                .FirstOrDefaultAsync(c => c.Id == req.CustomerId && c.Merchants.Any(m => m.Id == req.MerchantId), ct);
            if (customer is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }
        }

        // 3. the number to charge: the customer's saved number, otherwise the one typed in
        var accountNumber = customer?.PhoneNumber ?? Phone.ToLipilaFormat(req.AccountNumber)!;

        // 4. save the payment as Pending
        var payment = new Payment
        {
            MerchantId = req.MerchantId,
            WalletId = req.WalletId,
            CustomerId = customer?.Id,
            Amount = req.Amount,
            AccountNumber = accountNumber,
            Narration = req.Narration.Trim(),
            Status = PaymentStatus.Pending,
            Type = PaymentType.Collection
        };

        await db.Payments.AddAsync(payment, ct);
        await db.SaveChangesAsync(ct);

        // 5. send it to Lipila so the customer's phone is asked to approve
        LipilaPaymentResponse result;
        try
        {
            result = await $"{lipila.Value.BaseUrl}{lipila.Value.CollectionPath}"
                .WithHeader("x-api-key", apiKey)
                .PostJsonAsync(new PaymentRequest
                {
                    ReferenceId = payment.Id.ToString(),
                    Amount = payment.Amount,
                    Narration = payment.Narration,
                    AccountNumber = payment.AccountNumber,
                    Currency = "ZMW",
                    Email = customer?.Email ?? string.Empty,
                })
                .ReceiveJson<LipilaPaymentResponse>();
        }
        catch (FlurlHttpException ex)
        {
            var reason = await ex.GetResponseStringAsync();

            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);

            AddError($"Lipila rejected the request: {reason}");
            await Send.ErrorsAsync(502, ct);
            return;
        }

        // 6. reply. The status stays Pending until GET /collection/{id}/status settles it.
        await Send.ResponseAsync(new CollectionResponse
        {
            PaymentId = payment.Id,
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Message = string.IsNullOrWhiteSpace(result.Message)
                ? "Payment request sent. Waiting for the customer to approve."
                : result.Message,
            Created = payment.CreatedAt.ToOffset(TimeSpan.FromHours(2)).ToString("dd MMM yyyy, HH:mm")
        }, 201, ct);
    }
}