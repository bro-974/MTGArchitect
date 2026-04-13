var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
