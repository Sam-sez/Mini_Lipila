using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class GetMerchantWalletsEndpoint(AppDbContext db) : EndpointWithoutRequest<List<WalletResponse>>
{
    public override void Configure()
    {
        Get("/merchants/{id}/wallets");
        Summary(s =>
        {
            s.Description = "`this fetches the wallets under this merchant`";
            s.Summary = "Fetch all wallets of merchant";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var merchantExists = await db.Merchants.AnyAsync(m => m.Id == id, ct);
        if (!merchantExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var wallets = await db.Wallets
            .Where(w => w.MerchantId == id)
            .Select(w => new WalletResponse { Id = w.Id, Balance = w.Balance })
            .ToListAsync(ct);

        await Send.OkAsync(wallets, ct);
    }
}
