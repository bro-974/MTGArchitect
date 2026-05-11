using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MTGArchitect.Scryfall.Contracts;
using MTGArchitectServices.ApiService.Controllers;
using MTGArchitectServices.ApiService.Services;
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
        app.MapAiChat(api);
        app.MapChatSessions(api);
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
            var services = report.Entries.ToDictionary(
                e => e.Key == "self" ? "api" : e.Key,
                e => e.Value.Status.ToString());
            return Results.Ok(new { status = report.Status.ToString(), checkedAt = DateTimeOffset.UtcNow, services });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetServerStatus");

        return app;
    }

    public static WebApplication MapAiChat(this WebApplication app, RouteGroupBuilder apiRoot)
    {
        var secured_ai = apiRoot.MapGroup("/ai")
            .RequireAuthorization();

        secured_ai.MapGet("/health", async (
            [FromServices] MindHealthService mindHealthService,
            CancellationToken ct) =>
            Results.Ok(await mindHealthService.GetHealthAsync(ct)))
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("AiHealth");

        secured_ai.MapGet("/chat", async (
            string prompt,
            Guid sessionId,
            ClaimsPrincipal principal,
            HttpResponse response,
            [FromServices] MindServices mindServices,
            CancellationToken ct) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();
            await mindServices.StreamChatAsync(prompt, sessionId, userId, response, ct);
            return Results.Empty;
        })
        .WithName("AiChat");

        return app;
    }

    public static WebApplication MapChatSessions(this WebApplication app, RouteGroupBuilder apiRoot)
    {
        var chat = apiRoot.MapGroup("/chat/sessions")
            .RequireAuthorization();

        chat.MapPost(string.Empty, (
            CreateSessionRequest request,
            ClaimsPrincipal principal,
            [FromServices] ChatServices chatServices,
            CancellationToken ct) =>
            chatServices.CreateSessionAsync(request.DeckId, principal, ct))
        .WithName("CreateChatSession");

        chat.MapGet(string.Empty, (
            Guid deckId,
            ClaimsPrincipal principal,
            [FromServices] ChatServices chatServices,
            CancellationToken ct) =>
            chatServices.GetSessionsByDeckAsync(deckId, principal, ct))
        .WithName("GetChatSessions");

        chat.MapDelete("/{sessionId:guid}", (
            Guid sessionId,
            ClaimsPrincipal principal,
            [FromServices] ChatServices chatServices,
            CancellationToken ct) =>
            chatServices.DeleteSessionAsync(sessionId, principal, ct))
        .WithName("DeleteChatSession");

        chat.MapPatch("/{sessionId:guid}/name", (
            Guid sessionId,
            RenameSessionRequest request,
            ClaimsPrincipal principal,
            [FromServices] ChatServices chatServices,
            CancellationToken ct) =>
            chatServices.RenameSessionAsync(sessionId, request.Name, principal, ct))
        .WithName("RenameChatSession");

        chat.MapGet("/{sessionId:guid}/messages", (
            Guid sessionId,
            ClaimsPrincipal principal,
            [FromServices] ChatServices chatServices,
            CancellationToken ct) =>
            chatServices.GetSessionMessagesAsync(sessionId, principal, ct))
        .WithName("GetChatSessionMessages");

        return app;
    }

    private record CreateSessionRequest(Guid DeckId);
    private record RenameSessionRequest(string Name);
}