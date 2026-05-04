using System.Net;
using System.Net.Http.Json;

namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class DeckTests
{
    private static Guid _deckId;
    private static Guid _queryId;

    [Test, Order(1)]
    public async Task CreateDeck_ReturnsCreated()
    {
        var response = await IntegrationTestFixture.ApiClient.PostAsJsonAsync("/api/deck", new
        {
            name = "Integration Test Deck",
            type = "Standard",
            note = "Created by integration test"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var deck = await response.Content.ReadFromJsonAsync<DeckResponse>();
        Assert.That(deck, Is.Not.Null);
        Assert.That(deck!.Id, Is.Not.EqualTo(Guid.Empty));

        _deckId = deck.Id;
    }

    [Test, Order(2)]
    public async Task GetDeckById_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync($"/api/deck/{_deckId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deck = await response.Content.ReadFromJsonAsync<DeckResponse>();
        Assert.That(deck, Is.Not.Null);
        Assert.That(deck!.Id, Is.EqualTo(_deckId));
    }

    [Test, Order(3)]
    public async Task UpdateDeck_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.PutAsJsonAsync($"/api/deck/{_deckId}", new
        {
            name = "Integration Test Deck Updated",
            type = "Standard",
            note = "Updated by integration test"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deck = await response.Content.ReadFromJsonAsync<DeckResponse>();
        Assert.That(deck, Is.Not.Null);
        Assert.That(deck!.Name, Is.EqualTo("Integration Test Deck Updated"));
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
            cost = "{R}",
            isSideBoard = false
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test, Order(5)]
    public async Task AddQuerySearch_ReturnsCreated()
    {
        var queryId = Guid.NewGuid();
        var response = await IntegrationTestFixture.ApiClient.PostAsJsonAsync($"/api/deck/{_deckId}/query-search", new
        {
            id = queryId,
            queryJson = "{\"q\":\"bolt\"}",
            searchEngine = "Scryfall"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var query = await response.Content.ReadFromJsonAsync<QueryResponse>();
        Assert.That(query, Is.Not.Null);
        Assert.That(query!.Id, Is.EqualTo(queryId));

        _queryId = query.Id;
    }

    [Test, Order(6)]
    public async Task RemoveQuerySearch_ReturnsNoContent()
    {
        var response = await IntegrationTestFixture.ApiClient.DeleteAsync(
            $"/api/deck/{_deckId}/query-search/{_queryId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test, Order(7)]
    public async Task DeleteDeck_ReturnsNoContent()
    {
        var response = await IntegrationTestFixture.ApiClient.DeleteAsync($"/api/deck/{_deckId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    private record DeckResponse(Guid Id, string Name, string Type, string? Note);
    private record QueryResponse(Guid Id, string Query, string SearchEngine);
}
