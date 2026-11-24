using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class GetMerchantEndpoint(AppDbContext db) : Endpoint<GetMerchantReq, MerchantResponse>
{
    public override void Configure()
    {
        Get("/merchants/{id}");
        Summary(s =>
        {
            s.Description = "`this fetches one merchant and its wallets`";
            s.Summary = "Display merchant details";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetMerchantReq req, CancellationToken ct)
    {
        var merchant = await db.Merchants
            .Include(x => x.Wallets)
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (merchant is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new MerchantResponse
        {
            Id = merchant.Id,
            Name = merchant.Name,
            AccountNumber = merchant.AccountNumber,
            Email = merchant.Email,
            Wallets = merchant.Wallets.Select(w => new WalletResponse
            {
                Id = w.Id,
                Balance = w.Balance
            }).ToList()
        };

        await Send.OkAsync(response, ct);
    }
}
