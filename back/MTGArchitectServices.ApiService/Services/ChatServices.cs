using MTGArchitect.ChatMessage.Client.Services;
using System.Security.Claims;

namespace MTGArchitectServices.ApiService.Services;

public class ChatServices(IChatMessageClient chatMessageClient)
{
    public async Task<IResult> CreateSessionAsync(Guid deckId, ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        var sessions = await chatMessageClient.GetSessionsByDeckAsync(userId, deckId, ct);
        var sessionNumber = sessions.Count + 1;
        var displayName = $"Chat {sessionNumber}";

        var session = await chatMessageClient.CreateSessionAsync(userId, deckId, displayName, ct);
        return Results.Ok(session);
    }

    public async Task<IResult> GetSessionsByDeckAsync(Guid deckId, ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        var sessions = await chatMessageClient.GetSessionsByDeckAsync(userId, deckId, ct);
        return Results.Ok(sessions);
    }

    public async Task<IResult> DeleteSessionAsync(Guid sessionId, ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        await chatMessageClient.DeleteSessionAsync(sessionId, userId, ct);
        return Results.NoContent();
    }

    public async Task<IResult> RenameSessionAsync(Guid sessionId, string newName, ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        await chatMessageClient.RenameSessionAsync(sessionId, userId, newName, ct);
        return Results.NoContent();
    }

    public async Task<IResult> GetSessionMessagesAsync(Guid sessionId, ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        var data = await chatMessageClient.GetSessionWithMessagesAsync(sessionId, userId, int.MaxValue, ct);
        if (data is null) return Results.NotFound();
        return Results.Ok(data.Messages);
    }
}
