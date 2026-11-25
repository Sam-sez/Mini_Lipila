using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Requests;
using MiniLipila.Models.Responses;

namespace MiniLipila.Modules.Endpoints;

public class CreateMerchantUserEndpoint(UserManager<ApplicationUser> userManager, AppDbContext db)
    : Endpoint<CreateMerchantUserRequest, MerchantUserResponse>
{
    public override void Configure()
    {
        Post("/merchants/{merchantId}/users");
        Summary(s =>
        {
            s.Description = "`this creates a user who runs this merchant. The user is linked to the merchant and given the Merchant role. Admin only`";
            s.Summary = "Create a merchant user";
        });
        Roles(MiniLipila.Models.Roles.Admin);
    }

    public override async Task HandleAsync(CreateMerchantUserRequest req, CancellationToken ct)
    {
        // 1. the merchant must exist
        if (!await db.Merchants.AnyAsync(m => m.Id == req.MerchantId, ct))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 2. create the user already linked to the merchant
        var user = new ApplicationUser
        {
            UserName = req.Email,
            Email = req.Email,
            MerchantId = req.MerchantId
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                AddError(error.Description);

            await Send.ErrorsAsync(400, ct);
            return;
        }

        // 3. give them the Merchant role
        var roleResult = await userManager.AddToRoleAsync(user, MiniLipila.Models.Roles.Merchant);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
                AddError(error.Description);

            await Send.ErrorsAsync(500, ct);
            return;
        }

        await Send.ResponseAsync(new MerchantUserResponse
        {
            Id = user.Id,
            Email = user.Email!,
            MerchantId = req.MerchantId,
            Message = "Merchant user created."
        }, 201, ct);
    }
}