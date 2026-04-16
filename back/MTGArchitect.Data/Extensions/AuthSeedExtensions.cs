using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.Data.Data;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Extensions;

public static class AuthSeedExtensions
{
    private const string DefaultUserName = "Tester";
    private const string DefaultUserEmail = "nordyn@hotmail.fr";
    private const string DefaultUserPassword = "Aqw1!";

    public static async Task SeedDefaultAuthUserAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var hasMigrations = dbContext.Database.GetMigrations().Any();
        if (hasMigrations)
            await dbContext.Database.MigrateAsync(cancellationToken);
        else
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var existingUser = await userManager.FindByEmailAsync(DefaultUserEmail);
        if (existingUser is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = DefaultUserEmail,
            Email = DefaultUserEmail,
            DisplayName = DefaultUserName,
            EmailConfirmed = true,
            IsDeleted = false
        };

        var createResult = await userManager.CreateAsync(user);

        if (!createResult.Succeeded)
            throw new InvalidOperationException($"Unable to seed default auth user: {string.Join("; ", createResult.Errors.Select(x => x.Description))}");

        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, DefaultUserPassword);

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            throw new InvalidOperationException($"Unable to set seeded default user password: {string.Join("; ", updateResult.Errors.Select(x => x.Description))}");
    }
}
