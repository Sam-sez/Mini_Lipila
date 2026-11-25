using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;
using MiniLipila.Services;

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
        Roles(MiniLipila.Models.Roles.Merchant, MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(GetMerchantReq req, CancellationToken ct)
    {
        if (!User.CanAccess(req.Id))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

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
            Type = merchant.Type,
            Wallets = merchant.Wallets.Select(w => new WalletResponse
            {
                Id = w.Id,
                Balance = w.Balance
            }).ToList()
        };

        await Send.OkAsync(response, ct);
    }
}
