using Grpc.Core;
using MTGArchitect.AI.Contract;
using System.Runtime.CompilerServices;

namespace MTGArchitect.AI.Client.Services;

public interface IMindClient
{
    IAsyncEnumerable<ChatChunk> StreamChatAsync(string prompt, CancellationToken ct = default);
}

public class MindClient(MTGArchitect.AI.Service.MindService.MindServiceClient grpcClient) : IMindClient
{
    public async IAsyncEnumerable<ChatChunk> StreamChatAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var call = grpcClient.Chat(new MTGArchitect.AI.Service.ChatRequest { Prompt = prompt });
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
