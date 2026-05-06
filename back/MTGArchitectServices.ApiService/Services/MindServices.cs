using MTGArchitect.AI.Client.Services;
using System.Text.Json;

namespace MTGArchitectServices.ApiService.Services;

public class MindServices(IMindClient mindClient)
{
    public async Task StreamChatAsync(string prompt, HttpResponse response, CancellationToken ct)
    {
        response.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";

        await foreach (var chunk in mindClient.StreamChatAsync(prompt, ct))
        {
            if (string.IsNullOrEmpty(chunk.Content)) continue;
            var json = JsonSerializer.Serialize(new { content = chunk.Content, type = chunk.Type.ToString() });
            await response.WriteAsync($"data: {json}\n\n", ct);
            await response.Body.FlushAsync(ct);
        }
    }
}
