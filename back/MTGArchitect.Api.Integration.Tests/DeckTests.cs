using System.Net.Http.Json;
using System.Text.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class DeckTests
{
    private Guid _deckId;
    private Guid _queryId;

    [Test, Order(1)]
    public async Task CreateDeck_ReturnsCreated()
    {
        var response = await IntegrationTestFixture.ApiClient.PostAsJsonAsync("/api/deck", new
        {
            name = "Integration Test Deck",
            type = "Standard"
        });

        Assert.That((int)response.StatusCode, Is.EqualTo(201));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        _deckId = body.GetProperty("id").GetGuid();
        Assert.That(_deckId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test, Order(2)]
    public async Task GetDeckById_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync($"/api/deck/{_deckId}");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.GetProperty("id").GetGuid(), Is.EqualTo(_deckId));
        Assert.That(body.GetProperty("name").GetString(), Is.EqualTo("Integration Test Deck"));
    }

    [Test, Order(3)]
    public async Task UpdateDeck_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.PutAsJsonAsync($"/api/deck/{_deckId}", new
        {
            name = "Updated Integration Test Deck",
            type = "Commander"
        });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.GetProperty("name").GetString(), Is.EqualTo("Updated Integration Test Deck"));
        Assert.That(body.GetProperty("type").GetString(), Is.EqualTo("Commander"));
    }

    [Test, Order(4)]
    public async Task AddCardToDeck_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.PostAsJsonAsync($"/api/deck/{_deckId}/card", new
        {
            cardName = "Lightning Bolt",
            scryFallId = "e3285e6b-3e79-4d7c-bf96-d920f973b122",
            quantity = 4,
            type = "Instant",
            cost = "R",
            isSideBoard = false
        });

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.GetProperty("cardName").GetString(), Is.EqualTo("Lightning Bolt"));
        Assert.That(body.GetProperty("quantity").GetInt32(), Is.EqualTo(4));
    }

    [Test, Order(5)]
    public async Task AddQuerySearch_ReturnsCreated()
    {
        var queryId = Guid.NewGuid();
        var response = await IntegrationTestFixture.ApiClient.PostAsJsonAsync($"/api/deck/{_deckId}/query-search", new
        {
            id = queryId,
            queryJson = "{}",
            searchEngine = "Scryfall"
        });

        Assert.That((int)response.StatusCode, Is.EqualTo(201));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        _queryId = body.GetProperty("id").GetGuid();
        Assert.That(_queryId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test, Order(6)]
    public async Task RemoveQuerySearch_ReturnsNoContent()
    {
        var response = await IntegrationTestFixture.ApiClient.DeleteAsync($"/api/deck/{_deckId}/query-search/{_queryId}");

        Assert.That((int)response.StatusCode, Is.EqualTo(204));
    }

    [Test, Order(7)]
    public async Task DeleteDeck_ReturnsNoContent()
    {
        var response = await IntegrationTestFixture.ApiClient.DeleteAsync($"/api/deck/{_deckId}");

        Assert.That((int)response.StatusCode, Is.EqualTo(204));
    }
}
