using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLipila.Models;

namespace MiniLipila.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // one customer per phone number across the whole platform, enforced by the database
        builder.HasIndex(c => c.PhoneNumber).IsUnique();
    }
}
