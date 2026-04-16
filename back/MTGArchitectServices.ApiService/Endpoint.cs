using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MTGArchitect.Scryfall.Contracts;
using MTGArchitectServices.ApiService.Controllers;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        app.MapGet("/", () => "API service is running.");

        app.MapPublicHealth(api);

        app.MapPublicSearch(api);
        app.MapPrivatePing(api);

        app.MapDefaultEndpoints();

        return app;
    }

    private static WebApplication MapPrivatePing(this WebApplication app, RouteGroupBuilder apiRoot)
    {
        var auth = apiRoot.MapGroup("/auth")
            .RequireAuthorization();

        auth.MapGet("/logged", async (CancellationToken cancellationToken) =>
        {
            return Results.Ok();
        })
        .WithName("IsLogged");

        return app;
    }


    private static WebApplication MapPublicSearch(this WebApplication app, RouteGroupBuilder apiRoot)
    {
        var cards = apiRoot.MapGroup("/cards");

        cards.MapGet("/search", async (
            string q,
            int? pageSize,
            [FromServices] SearchServices searchController,
            CancellationToken cancellationToken) =>
        {
            return await searchController.QuerySearch(q, pageSize, cancellationToken);
        })
        .WithName("SearchCards");

        cards.MapPost("/search/advanced", async (
            CardQuerySearch body,
            [FromServices] SearchServices searchController,
            CancellationToken cancellationToken) =>
        {
            return await searchController.AdvancedSearch(body, cancellationToken);
        })
        .WithName("AdvancedSearchCards");

        return app;
    }

    private static WebApplication MapPublicHealth(this WebApplication app, RouteGroupBuilder apiRoot)
    {
        app.MapGrpcHealthChecksService();


        apiRoot.MapGet("/server-status", async (HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
        {
            var report = await healthCheckService.CheckHealthAsync(cancellationToken);
            return Results.Ok(new { status = report.Status.ToString(), checkedAt = DateTimeOffset.UtcNow });
        })
        .WithName("GetServerStatus");

        return app;
    }
}