using Microsoft.Extensions.DependencyInjection;
using MTGArchitect.ChatMessage.Client.Services;

namespace MTGArchitect.ChatMessage.Client;

public static class Module
{
    public static IServiceCollection AddChatMessageClient(this IServiceCollection services, Uri chatMessageService)
    {
        services.AddGrpcClient<MTGArchitect.ChatMessage.Service.ChatMessageService.ChatMessageServiceClient>(options =>
        {
            options.Address = chatMessageService;
        });
        services.AddScoped<IChatMessageClient, ChatMessageClient>();
        return services;
    }
}
