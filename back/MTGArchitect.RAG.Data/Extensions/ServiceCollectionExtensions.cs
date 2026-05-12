using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.RAG.Data.Data;
using MTGArchitect.RAG.Data.Repositories;
using MTGArchitect.RAG.Data.Services;

namespace MTGArchitect.RAG.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRagData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RagDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector());
        });

        services.AddScoped<ICardEmbeddingRepository, CardEmbeddingRepository>();

        return services;
    }

    public static IServiceCollection AddAgent(this IServiceCollection services, string lmStudioUri)
    {
        services.AddHttpClient("embedding", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("embedding");
            return new EmbeddingService(http, lmStudioUri);
        });

        services.AddScoped<ICardSearchService, CardSearchService>();

        return services;
    }
}
