using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models.Responses;
using MiniLipila.Services;

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
        Roles(MiniLipila.Models.Roles.Merchant, MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var merchantId = Route<Guid>("merchantId");

        if (!User.CanAccess(merchantId))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        if (!await db.Merchants.AnyAsync(m => m.Id == merchantId, ct))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var customers = await db.Customers
            .Where(c => c.Merchants.Any(m => m.Id == merchantId))
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        var response = customers.Select(c => new CustomerResponse
        {
            Id = c.Id,
            Name = c.Name,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            CustomerType = c.CustomerType,
            Created = c.CreatedAt.ToOffset(TimeSpan.FromHours(2)).ToString("dd MMM yyyy, HH:mm")
        }).ToList();

        await Send.OkAsync(response, ct);
    }
}
