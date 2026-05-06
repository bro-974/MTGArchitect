using System.Net.Http.Json;
using System.Text.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class AiChatTests
{
    [Test]
    public async Task StreamChat_WithoutToken_ReturnsUnauthorized()
    {
        var response = await IntegrationTestFixture.AnonClient.GetAsync("/api/ai/chat?prompt=hello");

        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task StreamChat_WithToken_ReturnsEventStream()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync(
            "/api/ai/chat?prompt=hello",
            HttpCompletionOption.ResponseHeadersRead);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/event-stream"));
    }

    [Test]
    [CancelAfter(120_000)]
    public async Task StreamChat_WithToken_ReceivesAtLeastOneValidChunk(CancellationToken ct)
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync(
            "/api/ai/chat?prompt=Reply+with+one+word+only",
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? firstChunk = null;
        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null) break;
            if (!line.StartsWith("data: ")) continue;

            firstChunk = line["data: ".Length..];
            break;
        }

        Assert.That(firstChunk, Is.Not.Null.And.Not.Empty, "Expected at least one SSE data line");

        var doc = JsonDocument.Parse(firstChunk!);
        Assert.That(doc.RootElement.TryGetProperty("content", out _), Is.True, "Chunk must have 'content' field");
        Assert.That(doc.RootElement.TryGetProperty("type", out var typeEl), Is.True, "Chunk must have 'type' field");
        Assert.That(new[] { "Reasoning", "Answer", "Metadata" }, Does.Contain(typeEl.GetString()));
    }
}
