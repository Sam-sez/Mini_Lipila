using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class CreateWalletEndpoint(AppDbContext db) : EndpointWithoutRequest<WalletResponse>
{
    public override void Configure()
    {
        Post("/merchants/{merchantId}/wallets");
        Summary(s =>
        {
            s.Description = "`this adds one empty wallet to a merchant`";
            s.Summary = "Add a wallet to a merchant";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchantId = Route<Guid>("merchantId");

        if (!await db.Merchants.AnyAsync(m => m.Id == merchantId, ct))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Balance = 0
        };

        db.Wallets.Add(wallet);
        await db.SaveChangesAsync(ct);

        await Send.ResponseAsync(new WalletResponse
        {
            Id = wallet.Id,
            Balance = wallet.Balance
        }, 201, ct);
    }
}