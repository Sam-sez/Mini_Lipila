using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;

namespace MiniLipila.Modules.Endpoints;

public class GetMerchantsEndpoint(AppDbContext db) : EndpointWithoutRequest<List<Merchant>>
{
    public override void Configure()
    {
        Get("/merchants");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchants = await db.Merchants.ToListAsync(ct);
        await Send.OkAsync(merchants, ct);
    }
}