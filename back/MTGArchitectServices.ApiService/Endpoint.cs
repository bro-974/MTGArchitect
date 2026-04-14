using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MTGArchitect.Scryfall.Contracts;
using MTGArchitectServices.ApiService.Controllers;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGrpcHealthChecksService();

        app.MapGet("/", () => "API service is running.");

        app.MapGet("/api/server-status", async (HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
        {
            var report = await healthCheckService.CheckHealthAsync(cancellationToken);
            return Results.Ok(new { status = report.Status.ToString(), checkedAt = DateTimeOffset.UtcNow });
        })
        .WithName("GetServerStatus");

        app.MapGet("/api/cards/search", async (
            string q,
            int? pageSize,
            [FromServices] SearchController searchController,
            CancellationToken cancellationToken) =>
        {
            return await searchController.QuerySearch(q, pageSize, cancellationToken); 
        })
        .WithName("SearchCards");

        app.MapPost("/api/cards/search/advanced", async (
            CardQuerySearch body,
            [FromServices] SearchController searchController,
            CancellationToken cancellationToken) =>
        {
            return await searchController.AdvancedSearch(body, cancellationToken);
        })
        .WithName("AdvancedSearchCards");

        app.MapDefaultEndpoints();

        return app;
    }
}