using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class CreateCustomerEndpoint(AppDbContext db) : Endpoint<CreateCustomerRequest, CustomerResponse>
{
    public override void Configure()
    {
        Post("/merchants/{merchantId}/customers");
        Summary(s =>
        {
            s.Description = "`this adds a customer to a merchant`";
            s.Summary = "Create a customer";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            AddError(r => r.Name, "Name is required");

        var phone = Phone.ToLipilaFormat(req.PhoneNumber);
        if (phone is null)
            AddError(r => r.PhoneNumber, "Enter a Zambian number like 0979086825 or 260979086825");

        ThrowIfAnyErrors();

        if (!await db.Merchants.AnyAsync(m => m.Id == req.MerchantId, ct))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var alreadyThere = await db.Customers
            .AnyAsync(c => c.MerchantId == req.MerchantId && c.PhoneNumber == phone, ct);
        if (alreadyThere)
        {
            AddError("This merchant already has a customer with that phone number");
            await Send.ErrorsAsync(409, ct);
            return;
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            MerchantId = req.MerchantId,
            Name = req.Name.Trim(),
            PhoneNumber = phone!,
            Email = req.Email.Trim()
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);

        await Send.ResponseAsync(new CustomerResponse
        {
            Id = customer.Id,
            MerchantId = customer.MerchantId,
            Name = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Created = customer.CreatedAt.ToOffset(TimeSpan.FromHours(2)).ToString("dd MMM yyyy, HH:mm")
        }, 201, ct);
    }
}
