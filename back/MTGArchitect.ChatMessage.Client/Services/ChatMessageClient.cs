using Grpc.Core;
using MTGArchitect.ChatMessage.Contracts;

namespace MTGArchitect.ChatMessage.Client.Services;

internal sealed class ChatMessageClient(MTGArchitect.ChatMessage.Service.ChatMessageService.ChatMessageServiceClient grpcClient) : IChatMessageClient
{
    public async Task<ChatSessionDto> CreateSessionAsync(string userId, Guid deckId, string displayName, CancellationToken cancellationToken = default)
    {
        var reply = await grpcClient.CreateSessionAsync(new MTGArchitect.ChatMessage.Service.CreateSessionRequest
        {
            UserId = userId,
            DeckId = deckId.ToString(),
            DisplayName = displayName
        }, cancellationToken: cancellationToken);

        return MapSession(reply);
    }

    public async Task<IReadOnlyList<ChatSessionDto>> GetSessionsByDeckAsync(string userId, Guid deckId, CancellationToken cancellationToken = default)
    {
        var reply = await grpcClient.GetSessionsByDeckAsync(new MTGArchitect.ChatMessage.Service.GetSessionsByDeckRequest
        {
            UserId = userId,
            DeckId = deckId.ToString()
        }, cancellationToken: cancellationToken);

        return reply.Sessions.Select(MapSession).ToList();
    }

    public async Task<ChatSessionWithMessagesDto?> GetSessionWithMessagesAsync(Guid sessionId, string userId, int limit, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await grpcClient.GetSessionWithMessagesAsync(new MTGArchitect.ChatMessage.Service.GetSessionWithMessagesRequest
            {
                SessionId = sessionId.ToString(),
                UserId = userId,
                Limit = limit
            }, cancellationToken: cancellationToken);

            return new ChatSessionWithMessagesDto
            {
                Session = MapSession(reply.Session),
                Messages = reply.Messages.Select(m => new ChatMessageDto
                {
                    Id = Guid.Parse(m.Id),
                    UserPrompt = m.UserPrompt,
                    Answer = m.Answer,
                    CreatedAt = DateTime.Parse(m.CreatedAt)
                }).ToList()
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddMessageAsync(Guid sessionId, string userPrompt, string answer, CancellationToken cancellationToken = default)
    {
        await grpcClient.AddMessageAsync(new MTGArchitect.ChatMessage.Service.AddMessageRequest
        {
            SessionId = sessionId.ToString(),
            UserPrompt = userPrompt,
            Answer = answer
        }, cancellationToken: cancellationToken);
    }

    public async Task DeleteSessionAsync(Guid sessionId, string userId, CancellationToken cancellationToken = default)
    {
        await grpcClient.DeleteSessionAsync(new MTGArchitect.ChatMessage.Service.DeleteSessionRequest
        {
            SessionId = sessionId.ToString(),
            UserId = userId
        }, cancellationToken: cancellationToken);
    }

    public async Task RenameSessionAsync(Guid sessionId, string userId, string newName, CancellationToken cancellationToken = default)
    {
        await grpcClient.RenameSessionAsync(new MTGArchitect.ChatMessage.Service.RenameSessionRequest
        {
            SessionId = sessionId.ToString(),
            UserId = userId,
            NewName = newName
        }, cancellationToken: cancellationToken);
    }

    private static ChatSessionDto MapSession(MTGArchitect.ChatMessage.Service.SessionReply reply) => new()
    {
        Id = Guid.Parse(reply.Id),
        DisplayName = reply.DisplayName,
        CreatedAt = DateTime.Parse(reply.CreatedAt)
    };
}
