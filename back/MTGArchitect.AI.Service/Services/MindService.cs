using Grpc.Core;
using OpenAI.Chat;
using System.ClientModel.Primitives;
using System.Text.Json;

namespace MTGArchitect.AI.Service.Services;

public class MindServiceHandler(ChatClient chatClient) : MindService.MindServiceBase
{
    public override async Task Chat(
        ChatRequest request,
        IServerStreamWriter<ChatResponseStream> responseStream,
        ServerCallContext context)
    {
        var messages = new List<ChatMessage> { new UserChatMessage(request.Prompt) };

        await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, cancellationToken: context.CancellationToken))
        {
            // Thinking tokens — DeepSeek-R1 returns them in reasoning_content (non-standard field)
            var reasoningChunk = ExtractReasoningContent(update);
            if (!string.IsNullOrEmpty(reasoningChunk))
            {
                await responseStream.WriteAsync(new ChatResponseStream
                {
                    ReasoningChunk = reasoningChunk
                });
            }

            // Standard answer tokens
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    await responseStream.WriteAsync(new ChatResponseStream
                    {
                        AnswerChunk = part.Text
                    });
                }
            }
        }
    }

    // reasoning_content is a DeepSeek-specific non-standard field — not exposed by the OpenAI SDK
    // We parse it from the raw JSON of each streaming update
    private static string? ExtractReasoningContent(StreamingChatCompletionUpdate update)
    {
        try
        {
            var raw = ModelReaderWriter.Write(update, ModelReaderWriterOptions.Json);
            using var doc = JsonDocument.Parse(raw.ToMemory());
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var delta = choices[0].GetProperty("delta");
                if (delta.TryGetProperty("reasoning_content", out var rc))
                    return rc.GetString();
            }
        }
        catch { /* non-fatal — skip reasoning on parse error */ }
        return null;
    }
}
