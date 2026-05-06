using Microsoft.Extensions.DependencyInjection;

namespace MTGArchitect.Scryfall.Client;

public static class Module
{
    public static IServiceCollection AddScryfallClient(this IServiceCollection services, Uri scryfallService)
    {
        //services.AddGrpcClient<>(options =>
        //{
        //    options.Address = scryfallService;
        //});
        //services.AddScoped<>();
        return services;
    }
}

