using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class ListMerchantsEndpoint(AppDbContext db) : EndpointWithoutRequest<List<MerchantResponse>>
{
    public override void Configure()
    {
        Get("/merchants");
        Summary(s =>
        {
            s.Description = "`this lists all merchants with their wallets`";
            s.Summary = "Lists all merchants";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchants = await db.Merchants
            .Select(x => new MerchantResponse
            {
                Id = x.Id,
                Name = x.Name,
                AccountNumber = x.AccountNumber,
                Email = x.Email,
                Wallets = x.Wallets.Select(w => new WalletResponse
                {
                    Id = w.Id,
                    Balance = w.Balance
                }).ToList()
            })
            .ToListAsync(ct);

        await Send.OkAsync(merchants, ct);
    }
}