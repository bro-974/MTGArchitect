using MTGArchitect.ChatMessage.Contracts;

namespace MTGArchitect.ChatMessage.Client.Services;

public interface IChatMessageClient
{
    Task<ChatSessionDto> CreateSessionAsync(string userId, Guid deckId, string displayName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatSessionDto>> GetSessionsByDeckAsync(string userId, Guid deckId, CancellationToken cancellationToken = default);
    Task<ChatSessionWithMessagesDto?> GetSessionWithMessagesAsync(Guid sessionId, string userId, int limit, CancellationToken cancellationToken = default);
    Task AddMessageAsync(Guid sessionId, string userPrompt, string answer, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(Guid sessionId, string userId, CancellationToken cancellationToken = default);
    Task RenameSessionAsync(Guid sessionId, string userId, string newName, CancellationToken cancellationToken = default);
}
