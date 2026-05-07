using MTGArchitect.Scryfall.Service;
using MTGArchitect.Scryfall.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();
builder.Services.AddScoped<ICardController,CardController>();
builder.Services.AddHttpClient("scryfall", client =>
{
	client.BaseAddress = new Uri("https://api.scryfall.com/");
	client.DefaultRequestHeaders.UserAgent.ParseAdd("MTGArchitect/1.0");
	client.DefaultRequestHeaders.Add("Accept", "*/*");
});

var app = builder.Build();

app.MapGrpcHealthChecksService();
app.MapGrpcService<ScryfallCardSearchService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.MapDefaultEndpoints();

app.Run();
