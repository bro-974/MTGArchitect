using MTGArchitect.AI.Service.Services;
using MTGArchitect.RAG.Data.Data;
using MTGArchitect.RAG.Data.Extensions;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();
builder.Services.AddHttpClient("lmstudio-health");
builder.Services.AddSingleton<LmStudioHealthCheck>();
builder.Services.AddHealthChecks()
    .AddCheck<LmStudioHealthCheck>("lmstudio");

var lmStudioUri = builder.Configuration["LmStudioUri"]
    ?? throw new InvalidOperationException("LmStudioUri configuration is missing.");

var openAIClient = new OpenAIClient(
    new ApiKeyCredential("lmstudio"),
    new OpenAIClientOptions { Endpoint = new Uri(lmStudioUri) });

builder.Services.AddSingleton(openAIClient.GetChatClient("deepseek-r1"));

var ragConnectionString = builder.Configuration.GetConnectionString("ragdb")
    ?? throw new InvalidOperationException("Connection string 'ragdb' is missing.");

builder.Services.AddRagData(ragConnectionString);
builder.Services.AddAgent(lmStudioUri);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ragDb = scope.ServiceProvider.GetRequiredService<RagDbContext>();
    await ragDb.Database.EnsureCreatedAsync();
}

app.MapDefaultEndpoints();
app.MapGrpcHealthChecksService();
app.MapGrpcService<MindServiceHandler>();
app.MapAiServiceEndpoints();

app.Run();
