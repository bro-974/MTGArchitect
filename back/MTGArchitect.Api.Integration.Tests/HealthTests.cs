using System.Text.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class HealthTests
{
    [Test]
    public async Task GetServerStatus_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/server-status");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAiHealth_ReturnsDependencyStatuses()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/ai/health");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(document.RootElement.TryGetProperty("status", out _), Is.True);
        Assert.That(document.RootElement.TryGetProperty("dependencies", out var dependencies), Is.True);
        Assert.That(dependencies.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(dependencies.GetArrayLength(), Is.EqualTo(2));
    }
}
