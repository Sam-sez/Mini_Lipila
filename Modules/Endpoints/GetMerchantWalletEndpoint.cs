using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;

namespace MiniLipila.Modules.Endpoints;

public class GetMerchantWalletEndpoint(AppDbContext db) : EndpointWithoutRequest<WalletResponse>
{
    public override void Configure()
    {
        Get("/merchants/{merchantId}/wallets/{walletId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchantId = Route<Guid>("merchantId");
        var walletId = Route<Guid>("walletId");

        var wallet = await db.Wallets
            .Where(w => w.Id == walletId && w.MerchantId == merchantId)
            .Select(w => new WalletResponse { Id = w.Id, Balance = w.Balance })
            .FirstOrDefaultAsync(ct);

        if (wallet is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(wallet, ct);
    }
}