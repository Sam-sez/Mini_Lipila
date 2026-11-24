using System.Text.Json.Serialization;
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

public class InitiateCollectionEndpoint(AppDbContext db, IConfiguration config, IOptions<LipilaSettings> lipila) : Endpoint<CollectionReq, CollectionResponse>
{
    public override void Configure()
    {
        Post("/collection");
        Summary(s =>
        {
            s.Description = "`this asks Lipila to collect money from a customer's mobile money into a merchant's wallet`";
            s.Summary = "Start a collection";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CollectionReq req, CancellationToken ct)
    {
        // 1. check the request itself
        if (req.Amount <= 0)
            AddError(r => r.Amount, "Amount must be greater than 0");

        string? phone = null;
        if (req.CustomerId is null)
        {
            phone = Phone.ToLipilaFormat(req.AccountNumber);
            if (phone is null)
                AddError(r => r.AccountNumber, "Choose a customer, or enter a Zambian number like 0979086825");
        }

        ThrowIfAnyErrors();

        var apiKey = config["Lipila:ApiKey"]
                     ?? throw new InvalidOperationException("Lipila:ApiKey is not set");

        // 2. the merchant must exist
        var merchant = await db.Merchants
            .FirstOrDefaultAsync(x => x.Id == req.MerchantId, ct);
        if (merchant is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 3. the wallet must exist AND belong to this merchant
        var wallet = await db.Wallets
            .FirstOrDefaultAsync(x => x.Id == req.WalletId && x.MerchantId == req.MerchantId, ct);
        if (wallet is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 4. if a customer was chosen, they must belong to this merchant too
        Customer? customer = null;
        if (req.CustomerId is not null)
        {
            customer = await db.Customers
                .FirstOrDefaultAsync(c => c.Id == req.CustomerId && c.MerchantId == req.MerchantId, ct);
            if (customer is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }
            phone = customer.PhoneNumber;
        }

        // 5. save the payment as Pending
        var payment = new Payment
        {
            MerchantId = req.MerchantId,
            WalletId = req.WalletId,
            CustomerId = customer?.Id,
            Amount = req.Amount,
            AccountNumber = phone!,
            Status = PaymentStatus.Pending,
            Narration = string.IsNullOrWhiteSpace(req.Narration)
                ? $"Payment to {merchant.Name}"
                : req.Narration,
        };

        await db.Payments.AddAsync(payment, ct);
        await db.SaveChangesAsync(ct);

        // 6. send it to Lipila so the customer's phone is asked to approve
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

        // 7. reply. The status stays Pending until GET /collection/{id}/status settles it.
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
