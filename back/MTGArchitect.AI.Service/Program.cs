using MTGArchitect.AI.Service.Services;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();
builder.Services.AddHttpClient("lmstudio-health");
builder.Services.AddSingleton<LmStudioHealthCheck>();

var lmStudioUri = builder.Configuration["LmStudioUri"]
    ?? throw new InvalidOperationException("LmStudioUri configuration is missing.");

var openAIClient = new OpenAIClient(
    new ApiKeyCredential("lmstudio"),
    new OpenAIClientOptions { Endpoint = new Uri(lmStudioUri) });

builder.Services.AddSingleton(openAIClient.GetChatClient("deepseek-r1"));

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGrpcHealthChecksService();
app.MapGrpcService<MindServiceHandler>();
app.MapAiServiceEndpoints();

app.Run();
