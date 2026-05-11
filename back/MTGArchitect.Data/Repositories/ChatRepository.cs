using Microsoft.EntityFrameworkCore;
using MTGArchitect.Data.Data;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Repositories;

internal sealed class ChatRepository(AuthDbContext dbContext) : IChatRepository
{
    public async Task<int> CountSessionsForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatSessions
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<ChatSession> CreateSessionAsync(string userId, Guid deckId, string displayName, CancellationToken cancellationToken = default)
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeckId = deckId,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ChatSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<IEnumerable<ChatSession>> GetSessionsByDeckAsync(string userId, Guid deckId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DeckId == deckId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSession?> GetSessionWithMessagesAsync(Guid sessionId, string userId, int limit, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);

        if (session is null) return null;

        session.Messages = await dbContext.ChatMessages
            .AsNoTracking()
            .Where(x => x.ChatSessionId == sessionId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return session;
    }

    public async Task AddMessageAsync(Guid sessionId, string userPrompt, string answer, CancellationToken cancellationToken = default)
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatSessionId = sessionId,
            UserPrompt = userPrompt,
            Answer = answer,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ChatMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSessionAsync(Guid sessionId, string userId, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);

        if (session is null) return;

        dbContext.ChatSessions.Remove(session);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RenameSessionAsync(Guid sessionId, string userId, string newName, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);

        if (session is null) return;

        session.DisplayName = newName;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
