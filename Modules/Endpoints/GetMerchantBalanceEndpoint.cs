using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;

namespace MiniLipila.Modules.Endpoints;

public class MerchantBalanceResponse
{
    public Guid MerchantId { get; set; }
    public int WalletCount { get; set; }
    public decimal TotalBalance { get; set; }
}

public class GetMerchantBalanceEndpoint(AppDbContext db) : EndpointWithoutRequest<MerchantBalanceResponse>
{
    public override void Configure()
    {
        Get("/merchants/{id}/balance");
        Summary(s =>
        {
            s.Description = "`this fetches the balance of this merchant`";
            s.Summary = "Get a merchants balance";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        if (!await db.Merchants.AnyAsync(m => m.Id == id, ct))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var wallets = db.Wallets.Where(w => w.MerchantId == id);

        var response = new MerchantBalanceResponse
        {
            MerchantId = id,
            WalletCount = await wallets.CountAsync(ct),
            TotalBalance = await wallets.SumAsync(w => w.Balance, ct)
        };

        await Send.OkAsync(response, ct);
    }
}