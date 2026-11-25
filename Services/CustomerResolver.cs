using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using Npgsql;

namespace MiniLipila.Services;

public sealed record ResolveResult(Customer? Customer, KycStatus Status);

public class CustomerResolver(AppDbContext db, LipilaKycService kyc)
{
    public async Task<ResolveResult> ResolveAsync(Guid merchantId, string phone, CancellationToken ct)
    {
        // 1. is this person already known to the platform?
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phone, ct);
        var isNew = customer is null;

        // 2. no: ask Lipila who owns the number, then prepare a new customer
        if (customer is null)
        {
            var lookup = await kyc.LookupAsync(phone, ct);
            if (lookup.Status != KycStatus.Found)
                return new ResolveResult(null, lookup.Status);

            customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = lookup.Name,
                PhoneNumber = phone,
                Email = string.Empty,
                CustomerType = CustomerType.Individual
            };
            db.Customers.Add(customer);
        }

        // 3. is this merchant already linked to this person?
        var customerId = customer.Id;
        var alreadyLinked = !isNew && await db.Customers
            .AnyAsync(c => c.Id == customerId && c.Merchants.Any(m => m.Id == merchantId), ct);

        // 4. not linked yet: link them (this is the step production skips for existing accounts)
        if (!alreadyLinked)
        {
            var merchant = await db.Merchants.FirstAsync(m => m.Id == merchantId, ct);
            customer.Merchants.Add(merchant);
        }

        if (isNew || !alreadyLinked)
        {
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                               { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // another request saved the same person or link at the same moment.
                // Forget our attempt and start again: this time we will find theirs.
                db.ChangeTracker.Clear();
                return await ResolveAsync(merchantId, phone, ct);
            }
        }

        return new ResolveResult(customer, KycStatus.Found);
    }
}