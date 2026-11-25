using System.Security.Claims;
using MiniLipila.Models;

namespace MiniLipila.Services;

public static class MerchantAccess
{
    public const string ClaimType = "merchantId";

    // Admins can touch any merchant; a merchant user only their own
    public static bool CanAccess(this ClaimsPrincipal user, Guid merchantId)
        => user.IsInRole(Roles.Admin)
           || (Guid.TryParse(user.FindFirstValue(ClaimType), out var id) && id == merchantId);
}
