using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.Data.Data;
using MTGArchitect.Data.Models;
using MTGArchitect.Data.Repositories;
using MTGArchitect.Data.Services;

namespace MTGArchitect.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthData(this IServiceCollection services, string authConnectionString)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(authConnectionString);
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthDataService, AuthDataService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDeckRepository, DeckRepository>();

        return services;
    }
}
