using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MiniLipila.Models;

namespace MiniLipila.Services;

public class JwtTokenService(IConfiguration config)
{
    public (string Token, DateTimeOffset ExpiresAt) Create(ApplicationUser user, IEnumerable<string> roles)
    {
        var key = config["Jwt:Key"]
                  ?? throw new InvalidOperationException("Jwt:Key is not set");
        var minutes = config.GetValue<int>("Jwt:AccessTokenMinutes");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        // the merchant this user runs, so endpoints can check ownership from the token alone
        if (user.MerchantId is not null)
            claims.Add(new Claim(MerchantAccess.ClaimType, user.MerchantId.Value.ToString()));

        // one claim per role, so the server can check roles from the token alone
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
