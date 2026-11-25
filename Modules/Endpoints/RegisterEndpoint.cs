using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MiniLipila.Models;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class RegisterEndpoint(UserManager<ApplicationUser> userManager)
    : Endpoint<RegisterRequest, RegisterResponse>
{
    public override void Configure()
    {
        Post("/auth/register");
        Summary(s =>
        {
            s.Description = "`this creates a new user account. The password is hashed by Identity and never stored as plain text`";
            s.Summary = "Register a new user";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = req.Email,
            Email = req.Email
        };

        // Identity hashes the password, checks the strength rules and refuses a duplicate email
        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                AddError(error.Description);

            await Send.ErrorsAsync(400, ct);
            return;
        }

        // every new account starts as a Customer; only an Admin can promote them later
        var roleResult = await userManager.AddToRoleAsync(user, MiniLipila.Models.Roles.Customer);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
                AddError(error.Description);

            await Send.ErrorsAsync(500, ct);
            return;
        }

        await Send.ResponseAsync(new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Message = "Account created."
        }, 201, ct);
    }
}