using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MTGArchitect.Data.Repositories;

namespace MTGArchitect.ChatMessage.Service.Services;

public class ChatMessageGrpcService(IChatRepository chatRepository) : ChatMessageService.ChatMessageServiceBase
{
    public override async Task<SessionReply> CreateSession(CreateSessionRequest request, ServerCallContext context)
    {
        var deckId = Guid.Parse(request.DeckId);
        var session = await chatRepository.CreateSessionAsync(request.UserId, deckId, request.DisplayName, context.CancellationToken);

        return new SessionReply
        {
            Id = session.Id.ToString(),
            DisplayName = session.DisplayName,
            CreatedAt = session.CreatedAt.ToString("O")
        };
    }

    public override async Task<SessionListReply> GetSessionsByDeck(GetSessionsByDeckRequest request, ServerCallContext context)
    {
        var deckId = Guid.Parse(request.DeckId);
        var sessions = await chatRepository.GetSessionsByDeckAsync(request.UserId, deckId, context.CancellationToken);

        var reply = new SessionListReply();
        reply.Sessions.AddRange(sessions.Select(s => new SessionReply
        {
            Id = s.Id.ToString(),
            DisplayName = s.DisplayName,
            CreatedAt = s.CreatedAt.ToString("O")
        }));

        return reply;
    }

    public override async Task<SessionWithMessagesReply> GetSessionWithMessages(GetSessionWithMessagesRequest request, ServerCallContext context)
    {
        var sessionId = Guid.Parse(request.SessionId);
        var session = await chatRepository.GetSessionWithMessagesAsync(sessionId, request.UserId, request.Limit, context.CancellationToken);

        if (session is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Session {request.SessionId} not found."));

        var reply = new SessionWithMessagesReply
        {
            Session = new SessionReply
            {
                Id = session.Id.ToString(),
                DisplayName = session.DisplayName,
                CreatedAt = session.CreatedAt.ToString("O")
            }
        };

        reply.Messages.AddRange(session.Messages.Select(m => new ChatMessageReply
        {
            Id = m.Id.ToString(),
            UserPrompt = m.UserPrompt,
            Answer = m.Answer,
            CreatedAt = m.CreatedAt.ToString("O")
        }));

        return reply;
    }

    public override async Task<Empty> AddMessage(AddMessageRequest request, ServerCallContext context)
    {
        var sessionId = Guid.Parse(request.SessionId);
        await chatRepository.AddMessageAsync(sessionId, request.UserPrompt, request.Answer, context.CancellationToken);
        return new Empty();
    }

    public override async Task<Empty> DeleteSession(DeleteSessionRequest request, ServerCallContext context)
    {
        var sessionId = Guid.Parse(request.SessionId);
        await chatRepository.DeleteSessionAsync(sessionId, request.UserId, context.CancellationToken);
        return new Empty();
    }

    public override async Task<Empty> RenameSession(RenameSessionRequest request, ServerCallContext context)
    {
        var sessionId = Guid.Parse(request.SessionId);
        await chatRepository.RenameSessionAsync(sessionId, request.UserId, request.NewName, context.CancellationToken);
        return new Empty();
    }
}
