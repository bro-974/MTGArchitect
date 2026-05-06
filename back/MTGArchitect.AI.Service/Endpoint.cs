using Microsoft.Extensions.Diagnostics.HealthChecks;
using MTGArchitect.AI.Service.Services;

public static class EndpointExtensions
{
    public static WebApplication MapAiServiceEndpoints(this WebApplication app)
    {
        app.MapGet("/internal/status/lmstudio", async (LmStudioHealthCheck lmStudioHealthCheck, CancellationToken ct) =>
        {
            var result = await lmStudioHealthCheck.CheckHealthAsync(new HealthCheckContext(), ct);

            return Results.Ok(new
            {
                status = result.Status.ToString(),
                description = result.Description,
                checkedAt = DateTimeOffset.UtcNow
            });
        });

        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        return app;
    }
}