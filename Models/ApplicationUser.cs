using Microsoft.AspNetCore.Identity;

namespace MiniLipila.Models;

// A person who can log in to Mini Lipila.
// Identity already provides Id, Email, UserName and PasswordHash.
public class ApplicationUser : IdentityUser
{
    // the merchant this user runs; empty for Admins and Customers
    public Guid? MerchantId { get; set; }
    public Merchant? Merchant { get; set; }
}