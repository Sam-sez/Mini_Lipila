using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLipila.Models;

namespace MiniLipila.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // a payment must point at a real merchant, wallet and (optionally) customer.
        // Restrict: the database refuses to delete anything that still has payments.
        // EF finds each foreign key from the property name (Merchant -> MerchantId).
        builder.HasOne(p => p.Merchant).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Wallet).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Customer).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}