using System.Net.Http.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class CardSearchTests
{
    private const string KnownScryfallId = "e3285e6b-3e79-4d7c-bf96-d920f973b122"; // Lightning Bolt

    [Test]
    public async Task QuerySearch_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/cards/search?q=bolt");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var cards = await response.Content.ReadFromJsonAsync<CardResult[]>();
        Assert.That(cards, Is.Not.Null);
        Assert.That(cards, Is.Not.Empty);
    }

    [Test]
    public async Task AdvancedSearch_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.PostAsJsonAsync(
            "/api/cards/search/advanced", new { });

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task GetCardDetail_WhenExists_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync($"/api/cards/{KnownScryfallId}");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var card = await response.Content.ReadFromJsonAsync<CardDetailResult>();
        Assert.That(card, Is.Not.Null);
        Assert.That(card!.Id, Is.EqualTo(KnownScryfallId));
    }

    [Test]
    public async Task GetCardDetail_WhenNotFound_ReturnsNotFound()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/cards/nonexistent-id");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    private record CardResult(string Id, string Name);
    private record CardDetailResult(string Id, string Name);
}
