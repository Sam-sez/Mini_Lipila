using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models.Responses;
using MiniLipila.Services;

namespace MiniLipila.Modules.Endpoints;

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
        Roles(MiniLipila.Models.Roles.Merchant, MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        if (!User.CanAccess(id))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

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
