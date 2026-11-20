using FastEndpoints;
using MiniLipila.Data;
using MiniLipila.Models;

namespace MiniLipila.Modules.Endpoints;

public class GetMerchantEndpoint(AppDbContext db) : EndpointWithoutRequest<Merchant>
{
    public override void Configure()
    {
        Get("/merchants/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var merchant = await db.Merchants.FindAsync([id], ct);

        if (merchant is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(merchant, ct);
    }
}