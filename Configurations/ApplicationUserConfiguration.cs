using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniLipila.Models;

namespace MiniLipila.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // the database refuses to delete a merchant that still has a user linked
        builder.HasOne(u => u.Merchant)
            .WithMany()
            .HasForeignKey(u => u.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}