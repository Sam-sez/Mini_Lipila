using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models.Responses;
using MiniLipila.Services;

namespace MiniLipila.Modules.Endpoints;

public class GetMerchantWalletEndpoint(AppDbContext db) : EndpointWithoutRequest<WalletResponse>
{
    public override void Configure()
    {
        Get("/merchants/{merchantId}/wallets/{walletId}");
        Summary(s =>
        {
            s.Description = "`this fetches one wallet under this merchant`";
            s.Summary = "Fetch a single wallet";
        });
        Roles(MiniLipila.Models.Roles.Merchant, MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchantId = Route<Guid>("merchantId");
        var walletId = Route<Guid>("walletId");

        if (!User.CanAccess(merchantId))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var wallet = await db.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId && w.MerchantId == merchantId, ct);

        if (wallet is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new WalletResponse
        {
            Id = wallet.Id,
            Balance = wallet.Balance
        }, ct);
    }
}
