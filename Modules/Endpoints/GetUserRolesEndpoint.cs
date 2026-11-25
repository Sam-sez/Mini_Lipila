using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using MiniLipila.Models;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class GetUserRolesEndpoint(UserManager<ApplicationUser> userManager)
    : EndpointWithoutRequest<UserRolesResponse>
{
    public override void Configure()
    {
        Get("/users/{id}/roles");
        Summary(s =>
        {
            s.Description = "`this lists the roles of one user`";
            s.Summary = "Get a user's roles";
        });
        Roles(MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            await Send.NotFoundAsync(ct);
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