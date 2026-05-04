using System.Net.Http.Json;
using System.Text.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class CardSearchTests
{
    // Stable Scryfall ID for Lightning Bolt (Magic 2010)
    private const string KnownCardId = "e3285e6b-3e79-4d7c-bf96-d920f973b122";

    [Test]
    public async Task SearchCards_WithValidQuery_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/cards/search?q=bolt");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(body.GetArrayLength(), Is.GreaterThan(0));
    }

    [Test]
    public async Task AdvancedSearch_WithEmptyBody_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.PostAsJsonAsync("/api/cards/search/advanced", new { });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetCardDetail_WithKnownId_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync($"/api/cards/{KnownCardId}");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.GetProperty("name").GetString(), Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task GetCardDetail_WithUnknownId_ReturnsNotFound()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/cards/nonexistent-id-00000000");

        Assert.That((int)response.StatusCode, Is.EqualTo(404));
    }
}
