using System.Net.Http.Json;
using System.Text.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class UserTests
{
    [Test]
    public async Task GetPrivate_WithToken_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/user/private");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetPrivate_WithoutToken_ReturnsUnauthorized()
    {
        var response = await IntegrationTestFixture.AnonClient.GetAsync("/api/user/private");

        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task GetSettings_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/user/settings");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.GetProperty("displayName").GetString(), Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task GetDecks_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/decks/all");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.ValueKind, Is.EqualTo(JsonValueKind.Array));
    }
}
