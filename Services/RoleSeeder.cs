using Microsoft.AspNetCore.Identity;
using MiniLipila.Models;

namespace MiniLipila.Services;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var name in Roles.All)
        {
            // only create the role if it is not there yet
            if (!await roleManager.RoleExistsAsync(name))
                await roleManager.CreateAsync(new IdentityRole(name));
        }

        await SeedAdminAsync(services);
    }

    private static async Task SeedAdminAsync(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var email = config["Admin:Email"];
        var password = config["Admin:Password"];

        // no secrets set: skip, so the app still starts
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        // already there: do nothing, so restarting never creates a second one
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser { UserName = email, Email = email };

        var created = await userManager.CreateAsync(admin, password);
        if (created.Succeeded)
            await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}