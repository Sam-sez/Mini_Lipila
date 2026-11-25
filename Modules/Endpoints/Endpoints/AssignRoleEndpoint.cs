using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MiniLipila.Models;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class AssignRoleEndpoint(UserManager<ApplicationUser> userManager)
    : Endpoint<AssignRoleRequest, UserRolesResponse>
{
    public override void Configure()
    {
        Post("/users/{id}/roles");
        Summary(s =>
        {
            s.Description = "`this adds one role to a user. The role must be Admin, Merchant or Customer`";
            s.Summary = "Assign a role to a user";
        });
        Roles(MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(AssignRoleRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.Id);
        if (user is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (await userManager.IsInRoleAsync(user, req.Role))
        {
            AddError("This user already has that role.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var result = await userManager.AddToRoleAsync(user, req.Role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                AddError(error.Description);

            await Send.ErrorsAsync(400, ct);
            return;
        }

        var roles = await userManager.GetRolesAsync(user);

        await Send.OkAsync(new UserRolesResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        }, ct);
    }
}