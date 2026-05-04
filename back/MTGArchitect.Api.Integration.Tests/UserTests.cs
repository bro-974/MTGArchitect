using System.Net.Http.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class UserTests
{
    [Test]
    public async Task AuthSmoke_WithToken_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/user/private");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task AuthSmoke_WithoutToken_ReturnsUnauthorized()
    {
        var response = await IntegrationTestFixture.AnonClient.GetAsync("/api/user/private");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetSettings_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/user/settings");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var user = await response.Content.ReadFromJsonAsync<UserSettingsResult>();
        Assert.That(user, Is.Not.Null);
    }

    [Test]
    public async Task GetDecks_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/decks/all");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var decks = await response.Content.ReadFromJsonAsync<DeckResult[]>();
        Assert.That(decks, Is.Not.Null);
    }

    private record UserSettingsResult(string? DisplayName, string? Language, string? Theme);
    private record DeckResult(Guid Id, string Name);
}
