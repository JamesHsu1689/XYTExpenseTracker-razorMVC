using System;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker;

public class IdentitySeed
{
    public static async Task EnsureSeededAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1) Ensure the roles exist (idempotent)
        foreach (var r in new[] { "Admin", "Staff", "User" })
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));

        // 2) First admin from config (skip if not configured)
        var email = config["AdminSeed:Email"];
        var pwd   = config["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd)) return;

        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var res = await userMgr.CreateAsync(user, pwd);
            if (!res.Succeeded)
                throw new Exception(string.Join("; ", res.Errors.Select(e => e.Description)));
        }
        if (!await userMgr.IsInRoleAsync(user, "Admin"))
            await userMgr.AddToRoleAsync(user, "Admin");
    }
}
