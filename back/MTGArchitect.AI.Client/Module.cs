using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.AI.Client.Services;
using Grpc.Health.V1;

namespace MTGArchitect.AI.Client;

public static class Module
{
    public static IServiceCollection AddMindClient(this IServiceCollection services, Uri aiService)
    {
        services.AddGrpcClient<MTGArchitect.AI.Service.MindService.MindServiceClient>(options =>
        {
            options.Address = aiService;
        });
        services.AddGrpcClient<Health.HealthClient>(options =>
        {
            options.Address = aiService;
        });
        services.AddHttpClient<IMindHealthClient, MindHealthClient>(client =>
        {
            client.BaseAddress = aiService;
        });
        services.AddScoped<IMindClient, MindClient>();
        return services;
    }
}
