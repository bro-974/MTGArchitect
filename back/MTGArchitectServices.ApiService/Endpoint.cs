using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MTGArchitect.Scryfall.Contracts;
using MTGArchitectServices.ApiService.Controllers;
using System.Security.Claims;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        app.MapGet("/", () => "API service is running.");

        app.MapServer(api);
        app.MapPublicSearch(api);
        app.MapPrivateUserEndpoints(api);
        app.MapDefaultEndpoints();

        return app;
    }

    private static WebApplication MapPrivateUserEndpoints(this WebApplication app, RouteGroupBuilder apiRoot)
    {
        var secured = apiRoot.MapGroup(string.Empty)
            .RequireAuthorization();

        secured.MapGet("/user/private", async (CancellationToken cancellationToken) =>
        {
            await Task.CompletedTask;
            return Results.Ok();
        })
        .WithName("IsPrivate");

        secured.MapGet("/user/settings", (
            ClaimsPrincipal principal,
            UserService userService,
            CancellationToken cancellationToken) =>
            userService.GetSettingsAsync(principal, cancellationToken))
        .WithName("GetLoggedUserSettings");

        secured.MapGet("/decks/all", (
            ClaimsPrincipal principal,
            UserService userService,
            CancellationToken cancellationToken) =>
            userService.GetDecksAsync(principal, cancellationToken))
        .WithName("GetLoggedUserDecks");

        secured.MapGet("/deck/{id:guid}", (
            Guid id,
            ClaimsPrincipal principal,
            DeckService deckService,
            CancellationToken cancellationToken) =>
            deckService.GetByIdAsync(id, principal, cancellationToken))
        .WithName("GetDeckById");

        secured.MapPost("/deck", (
            DeckUpsertRequest request,
            ClaimsPrincipal principal,
            DeckService deckService,
            CancellationToken cancellationToken) =>
            deckService.CreateAsync(request, principal, cancellationToken))
        .WithName("CreateDeck");

        secured.MapPut("/deck/{id:guid}", (
            Guid id,
            DeckUpsertRequest request,
            ClaimsPrincipal principal,
            DeckService deckService,
            CancellationToken cancellationToken) =>
            deckService.UpdateAsync(id, request, principal, cancellationToken))
        .WithName("UpdateDeckById");

        secured.MapDelete("/deck/{id:guid}", (
            Guid id,
            ClaimsPrincipal principal,
            DeckService deckService,
            CancellationToken cancellationToken) =>
            deckService.DeleteAsync(id, principal, cancellationToken))
        .WithName("DeleteDeckById");

        secured.MapPost("/deck/{deckId:guid}/card", (
            Guid deckId,
            DeckCardAddRequest request,
            ClaimsPrincipal principal,
            DeckService deckService,
            CancellationToken cancellationToken) =>
            deckService.AddCardToDeckAsync(deckId, request, principal, cancellationToken))
        .WithName("AddCardToDeck");

        secured.MapPost("/deck/{deckId:guid}/query-search", (
            Guid deckId,
            QueryInfoUpsertRequest request,
            ClaimsPrincipal principal,
            DeckService deckService,
            CancellationToken cancellationToken) =>
            deckService.AddQuerySearchToDeckAsync(deckId, request, principal, cancellationToken))
        .WithName("AddQuerySearchToDeck");

        secured.MapDelete("/deck/{deckId:guid}/query-search/{queryId:guid}", (
            Guid deckId,
            Guid queryId,
            ClaimsPrincipal principal,
            DeckService deckService,
            CancellationToken cancellationToken) =>
            deckService.RemoveQuerySearchAsync(deckId, queryId, principal, cancellationToken))
        .WithName("RemoveQuerySearchFromDeck");

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

        cards.MapGet("/{id}", async (
            string id,
            [FromServices] SearchServices searchController,
            CancellationToken cancellationToken) =>
        {
            return await searchController.GetCardDetail(id, cancellationToken);
        })
        .WithName("GetCardDetail");

        return app;
    }

    private static WebApplication MapServer(this WebApplication app, RouteGroupBuilder apiRoot)
    {
        app.MapGrpcHealthChecksService();

        apiRoot.MapGet("/server-status", async (HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
        {
            var report = await healthCheckService.CheckHealthAsync(cancellationToken);
            return Results.Ok(new { status = report.Status.ToString(), checkedAt = DateTimeOffset.UtcNow });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetServerStatus");

        return app;
    }
}