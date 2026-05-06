using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.Data.Data;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Extensions;

public static class AuthSeedExtensions
{
    private const string AdminRole = "Admin";

    public static async Task SeedDefaultAuthUserAsync(
        this WebApplication app,
        string defaultEmail,
        string defaultPassword,
        string defaultDisplayName,
        string adminEmail,
        string adminPassword,
        string adminDisplayName,
        CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var hasMigrations = dbContext.Database.GetMigrations().Any();
        if (hasMigrations)
            await dbContext.Database.MigrateAsync(cancellationToken);
        else
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Pass 1 — ensure Admin role exists
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRole));
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Unable to create Admin role: {string.Join("; ", roleResult.Errors.Select(x => x.Description))}");
        }

        // Pass 2 — seed default test user (always sync password to current config)
        var existingDefault = await userManager.FindByEmailAsync(defaultEmail);
        if (existingDefault is null)
        {
            var defaultUser = new ApplicationUser
            {
                UserName = defaultEmail,
                Email = defaultEmail,
                DisplayName = defaultDisplayName,
                EmailConfirmed = true,
                IsDeleted = false
            };

            var createResult = await userManager.CreateAsync(defaultUser);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Unable to seed default auth user: {string.Join("; ", createResult.Errors.Select(x => x.Description))}");

            existingDefault = defaultUser;
        }

        existingDefault.PasswordHash = userManager.PasswordHasher.HashPassword(existingDefault, defaultPassword);
        var updateResult = await userManager.UpdateAsync(existingDefault);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException($"Unable to set seeded default user password: {string.Join("; ", updateResult.Errors.Select(x => x.Description))}");

        // Pass 3 — seed admin user
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = adminDisplayName,
                EmailConfirmed = true,
                IsDeleted = false
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Unable to seed admin user: {string.Join("; ", createResult.Errors.Select(x => x.Description))}");

            await userManager.AddToRoleAsync(adminUser, AdminRole);
        }
        else if (!await userManager.IsInRoleAsync(existingAdmin, AdminRole))
        {
            await userManager.AddToRoleAsync(existingAdmin, AdminRole);
        }
    }
}
