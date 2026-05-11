using MTGArchitect.ChatMessage.Service.Services;
using MTGArchitect.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var authConnectionString = builder.Configuration.GetConnectionString("authdb")
    ?? throw new InvalidOperationException("Connection string 'authdb' is missing.");

builder.Services.AddMessageData(authConnectionString);
builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();

var app = builder.Build();

app.MapGrpcHealthChecksService();
app.MapGrpcService<ChatMessageGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapDefaultEndpoints();

app.Run();
