using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.RAG.Data.Data;
using MTGArchitect.RAG.Data.Repositories;

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
}
