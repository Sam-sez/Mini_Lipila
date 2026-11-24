using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;

namespace MiniLipila.Modules.Endpoints;

public class ListCustomersEndpoint(AppDbContext db) : EndpointWithoutRequest<List<CustomerResponse>>
{
    public override void Configure()
    {
        Get("/merchants/{merchantId}/customers");
        Summary(s =>
        {
            s.Description = "`this lists the customers of one merchant`";
            s.Summary = "List a merchant's customers";
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

        var customers = await db.Customers
            .Where(c => c.MerchantId == merchantId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        var response = customers.Select(c => new CustomerResponse
        {
            Id = c.Id,
            MerchantId = c.MerchantId,
            Name = c.Name,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Created = c.CreatedAt.ToOffset(TimeSpan.FromHours(2)).ToString("dd MMM yyyy, HH:mm")
        }).ToList();

        await Send.OkAsync(response, ct);
    }
}