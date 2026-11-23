using FastEndpoints;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;

namespace MiniLipila.Modules.Endpoints;

public class CollectionStatusResponse
{
    public Guid PaymentId { get; set; }
    public string OurStatus { get; set; } = string.Empty;
    public string LipilaStatus { get; set; } = string.Empty;
    public bool WalletCredited { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CheckCollectionEndpoint(AppDbContext db, IConfiguration config)
    : EndpointWithoutRequest<CollectionStatusResponse>
{
    public override void Configure()
    {
        Get("/collection/{id}/status");
        Summary(s =>
        {
            s.Description = "`this asks Lipila what happened to a collection, updates it, and credits the wallet once if it succeeded`";
            s.Summary = "Check a collection's status";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var payment = await db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (payment is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Already settled: do not ask again, and never touch the wallet again.
        if (payment.Status != PaymentStatus.Pending)
        {
            await Send.OkAsync(new CollectionStatusResponse
            {
                PaymentId = payment.Id,
                OurStatus = payment.Status.ToString(),
                LipilaStatus = "not checked, already settled",
                WalletCredited = false,
                Message = "This payment is already settled."
            }, ct);
            return;
        }

        var apiKey = config["Lipila:ApiKey"]
            ?? throw new InvalidOperationException("Lipila:ApiKey is not set");

        LipilaPaymentResponse result;
        try
        {
            result = await "https://api.lipila.dev/api/v1/collections/check-status"
                .SetQueryParam("referenceId", payment.Id.ToString())
                .WithHeader("x-api-key", apiKey)
                .GetJsonAsync<LipilaPaymentResponse>();
        }
        catch (FlurlHttpException ex)
        {
            AddError($"Lipila could not check the payment: {await ex.GetResponseStringAsync()}");
            await Send.ErrorsAsync(502, ct);
            return;
        }

        var newStatus = result.Status switch
        {
            "Successful" => PaymentStatus.Successful,
            "Failed" => PaymentStatus.Failed,
            _ => PaymentStatus.Pending
        };

        var credited = false;

        if (newStatus != PaymentStatus.Pending)
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // Only one caller can move the payment away from Pending.
            var changed = await db.Payments
                .Where(p => p.Id == payment.Id && p.Status == PaymentStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, newStatus)
                    .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow), ct);

            if (changed == 1 && newStatus == PaymentStatus.Successful)
            {
                await db.Wallets
                    .Where(w => w.Id == payment.WalletId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(w => w.Balance, w => w.Balance + payment.Amount), ct);
                credited = true;
            }

            await tx.CommitAsync(ct);
        }

        await Send.OkAsync(new CollectionStatusResponse
        {
            PaymentId = payment.Id,
            OurStatus = newStatus.ToString(),
            LipilaStatus = result.Status,
            WalletCredited = credited,
            Message = result.Message
        }, ct);
    }
}