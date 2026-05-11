using MTGArchitect.AI.Client.Services;
using MTGArchitect.AI.Contract;
using MTGArchitect.ChatMessage.Client.Services;
using System.Text;
using System.Text.Json;

namespace MTGArchitectServices.ApiService.Services;

public class MindServices(IMindClient mindClient, IChatMessageClient chatMessageClient, IConfiguration configuration)
{
    public async Task StreamChatAsync(string prompt, Guid sessionId, string userId, HttpResponse response, CancellationToken ct)
    {
        var historyLimit = configuration.GetValue<int>("Chat:HistoryLimit", 10);

        var sessionData = await chatMessageClient.GetSessionWithMessagesAsync(sessionId, userId, historyLimit, ct);
        var history = sessionData?.Messages
            .Select(m => new ChatHistoryTurn(m.UserPrompt, m.Answer))
            .ToList();

        response.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";

        var answerBuilder = new StringBuilder();

        await foreach (var chunk in mindClient.StreamChatAsync(prompt, history, ct))
        {
            if (string.IsNullOrEmpty(chunk.Content)) continue;

            if (chunk.Type == ChunkType.Answer)
                answerBuilder.Append(chunk.Content);

            var json = JsonSerializer.Serialize(new { content = chunk.Content, type = chunk.Type.ToString() });
            await response.WriteAsync($"data: {json}\n\n", ct);
            await response.Body.FlushAsync(ct);
        }

        var fullAnswer = answerBuilder.ToString();
        if (!string.IsNullOrEmpty(fullAnswer))
            await chatMessageClient.AddMessageAsync(sessionId, prompt, fullAnswer, ct);
    }
}
