using MTGArchitect.Scryfall.Client;
using MTGArchitectServices.ApiService.Controllers;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddScoped<SearchController>();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend-dev", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScryfallClient(new Uri("https://scryfallservice"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseCors("frontend-dev");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapApiEndpoints();

app.Run();
