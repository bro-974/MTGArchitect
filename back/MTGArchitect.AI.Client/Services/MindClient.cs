using Grpc.Core;
using MTGArchitect.AI.Contract;
using System.Runtime.CompilerServices;

namespace MTGArchitect.AI.Client.Services;

public interface IMindClient
{
    IAsyncEnumerable<ChatChunk> StreamChatAsync(
        string prompt,
        IEnumerable<ChatHistoryTurn>? history = null,
        string? format = null,
        string? deckContext = null,
        CancellationToken ct = default);
}

public class MindClient(MTGArchitect.AI.Service.MindService.MindServiceClient grpcClient) : IMindClient
{
    public async IAsyncEnumerable<ChatChunk> StreamChatAsync(
        string prompt,
        IEnumerable<ChatHistoryTurn>? history = null,
        string? format = null,
        string? deckContext = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var request = new MTGArchitect.AI.Service.ChatRequest { Prompt = prompt };
        if (history is not null)
        {
            request.History.AddRange(history.Select(t => new MTGArchitect.AI.Service.ChatTurn
            {
                UserPrompt = t.UserPrompt,
                Answer = t.Answer
            }));
        }
        if (!string.IsNullOrEmpty(format))      request.Format = format;
        if (!string.IsNullOrEmpty(deckContext)) request.DeckContext = deckContext;

        using var call = grpcClient.Chat(request);
        await foreach (var chunk in call.ResponseStream.ReadAllAsync(ct))
        {
            yield return chunk.PayloadCase switch
            {
                MTGArchitect.AI.Service.ChatResponseStream.PayloadOneofCase.ReasoningChunk
                    => new ChatChunk(chunk.ReasoningChunk, ChunkType.Reasoning),
                MTGArchitect.AI.Service.ChatResponseStream.PayloadOneofCase.AnswerChunk
                    => new ChatChunk(chunk.AnswerChunk, ChunkType.Answer),
                _ => new ChatChunk(string.Empty, ChunkType.Metadata)
            };
        }
    }
}
