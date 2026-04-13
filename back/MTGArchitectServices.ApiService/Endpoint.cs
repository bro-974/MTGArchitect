using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class EndpointExtensions
{
    private static readonly string[] Summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGrpcHealthChecksService();

        app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                ))
                .ToArray();

            return forecast;
        })
        .WithName("GetWeatherForecast");

        app.MapGet("/api/server-status", async (HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
        {
            var report = await healthCheckService.CheckHealthAsync(cancellationToken);

            return Results.Ok(new
            {
                status = report.Status.ToString(),
                checkedAt = DateTimeOffset.UtcNow
            });
        })
        .WithName("GetServerStatus");

        app.MapDefaultEndpoints();

        return app;
    }

    private sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
