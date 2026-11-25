using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MiniLipila.Models;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;
using MiniLipila.Services;

namespace MiniLipila.Modules.Endpoints;

public class LoginEndpoint(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService jwt)
    : Endpoint<LoginRequest, LoginResponse>
{
    private const string BadLogin = "Invalid email or password.";

    public override void Configure()
    {
        Post("/auth/login");
        Summary(s =>
        {
            s.Description = "`this checks an email and password. A wrong email and a wrong password give the same answer`";
            s.Summary = "Log in";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null)
        {
            AddError(BadLogin);
            await Send.ErrorsAsync(401, ct);
            return;
        }

        // checks the password against the stored hash; true = count failed attempts toward lockout
        var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, true);

        if (result.IsLockedOut)
        {
            AddError("Too many failed attempts. Try again later.");
            await Send.ErrorsAsync(401, ct);
            return;
        }

        if (!result.Succeeded)
        {
            AddError(BadLogin);
            await Send.ErrorsAsync(401, ct);
            return;
        }

        var roles = await userManager.GetRolesAsync(user);
        var (token, expiresAt) = jwt.Create(user, roles);

        await Send.OkAsync(new LoginResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Message = "Login successful.",
            AccessToken = token,
            ExpiresAt = expiresAt.ToOffset(TimeSpan.FromHours(2)).ToString("dd MMM yyyy, HH:mm")
        }, ct);
    }
}