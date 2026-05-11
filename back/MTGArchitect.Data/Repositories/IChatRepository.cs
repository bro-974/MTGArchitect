using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Repositories;

public interface IChatRepository
{
    Task<int> CountSessionsForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<ChatSession> CreateSessionAsync(string userId, Guid deckId, string displayName, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatSession>> GetSessionsByDeckAsync(string userId, Guid deckId, CancellationToken cancellationToken = default);
    Task<ChatSession?> GetSessionWithMessagesAsync(Guid sessionId, string userId, int limit, CancellationToken cancellationToken = default);
    Task AddMessageAsync(Guid sessionId, string userPrompt, string answer, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(Guid sessionId, string userId, CancellationToken cancellationToken = default);
    Task RenameSessionAsync(Guid sessionId, string userId, string newName, CancellationToken cancellationToken = default);
}
