using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.Scryfall.Client.Services;
using MTGArchitect.Scryfall.Service.Contracts;

namespace MTGArchitect.Scryfall.Client;

public static class Module
{
    public static IServiceCollection AddScryfallClient(this IServiceCollection services, Uri scryfallService)
    {
        services.AddGrpcClient<CardSearch.CardSearchClient>(options =>
        {
            options.Address = scryfallService;
        });
        services.AddScoped<IScryfallClient, ScryfallClient>();
        return services;
    }
}

