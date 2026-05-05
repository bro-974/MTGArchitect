using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MTGArchitect.Data.Models;
using MTGArchitect.Data.Repositories;
using NSubstitute;

namespace MTGArchitectServices.ApiService.Tests;

[TestFixture]
public class DeckServiceTests
{
    private IDeckRepository _repository = null!;
    private DeckService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IDeckRepository>();
        _service = new DeckService(_repository);
    }

    // --- GetByIdAsync ---

    [Test]
    public async Task GetByIdAsync_WhenDeckExists_ReturnsOk()
    {
        var deck = CreateDeck();
        _repository.GetByIdWithDetailsAsync(deck.Id, "user-1", Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(deck);

        var result = await _service.GetByIdAsync(deck.Id, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
        Assert.That((result as IValueHttpResult)?.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_WhenDeckNotFound_ReturnsNotFound()
    {
        _repository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Deck?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetByIdAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    // --- CreateAsync ---

    [Test]
    public async Task CreateAsync_WithValidRequest_ReturnsCreated()
    {
        var request = new DeckUpsertRequest("Test Deck", "Standard", null, null, null,null,null);

        var result = await _service.CreateAsync(request, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(201));
        await _repository.Received(1).AddAsync(Arg.Any<Deck>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var request = new DeckUpsertRequest("Test Deck", "Standard", null, null, null, null, null);

        var result = await _service.CreateAsync(request, CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Deck>(), Arg.Any<CancellationToken>());
    }

    // --- UpdateAsync ---

    [Test]
    public async Task UpdateAsync_WhenDeckExists_ReturnsOk()
    {
        var deck = CreateDeck();
        _repository.GetByIdWithDetailsAsync(deck.Id, "user-1", Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(deck);
        var request = new DeckUpsertRequest("Updated", "Commander", null, null, null, null, null);

        var result = await _service.UpdateAsync(deck.Id, request, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateAsync_WhenDeckNotFound_ReturnsNotFound()
    {
        _repository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Deck?)null);
        var request = new DeckUpsertRequest("Updated", "Commander", null, null, null, null, null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), request, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task UpdateAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var request = new DeckUpsertRequest("Updated", "Commander", null, null, null, null, null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), request, CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    // --- DeleteAsync ---

    [Test]
    public async Task DeleteAsync_WhenDeckExists_ReturnsNoContent()
    {
        var deck = CreateDeck();
        _repository.GetByIdAsync(deck.Id, "user-1", Arg.Any<CancellationToken>())
            .Returns(deck);

        var result = await _service.DeleteAsync(deck.Id, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(204));
        await _repository.Received(1).DeleteAsync(deck, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DeleteAsync_WhenDeckNotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Deck?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid(), CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task DeleteAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    // --- AddQuerySearchToDeckAsync ---

    [Test]
    public async Task AddQuerySearchToDeckAsync_WhenDeckExists_ReturnsCreated()
    {
        var deck = CreateDeck();
        _repository.GetByIdAsync(deck.Id, "user-1", Arg.Any<CancellationToken>())
            .Returns(deck);
        var request = new QueryInfoUpsertRequest(null, "{}", "Scryfall");

        var result = await _service.AddQuerySearchToDeckAsync(deck.Id, request, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(201));
    }

    [Test]
    public async Task AddQuerySearchToDeckAsync_WhenDeckNotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Deck?)null);
        var request = new QueryInfoUpsertRequest(null, "{}", "Scryfall");

        var result = await _service.AddQuerySearchToDeckAsync(Guid.NewGuid(), request, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task AddQuerySearchToDeckAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var request = new QueryInfoUpsertRequest(null, "{}", "Scryfall");

        var result = await _service.AddQuerySearchToDeckAsync(Guid.NewGuid(), request, CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    // --- AddCardToDeckAsync ---

    [Test]
    public async Task AddCardToDeckAsync_WhenDeckExists_ReturnsOk()
    {
        var deck = CreateDeck();
        var card = new DeckCard { Id = Guid.NewGuid(), CardName = "Lightning Bolt", ScryFallId = "abc", Quantity = 1, Type = "Instant", Cost = "R" };
        _repository.GetByIdAsync(deck.Id, "user-1", Arg.Any<CancellationToken>())
            .Returns(deck);
        _repository.AddOrIncrementCardAsync(deck.Id, Arg.Any<DeckCard>(), Arg.Any<CancellationToken>())
            .Returns(card);
        var request = new DeckCardAddRequest("Lightning Bolt", "abc", 1, "Instant", "R", false);

        var result = await _service.AddCardToDeckAsync(deck.Id, request, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task AddCardToDeckAsync_WhenDeckNotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Deck?)null);
        var request = new DeckCardAddRequest("Lightning Bolt", "abc", 1, "Instant", "R", false);

        var result = await _service.AddCardToDeckAsync(Guid.NewGuid(), request, CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task AddCardToDeckAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var request = new DeckCardAddRequest("Lightning Bolt", "abc", 1, "Instant", "R", false);

        var result = await _service.AddCardToDeckAsync(Guid.NewGuid(), request, CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    // --- RemoveQuerySearchAsync ---

    [Test]
    public async Task RemoveQuerySearchAsync_WhenDeckExists_ReturnsNoContent()
    {
        var deck = CreateDeck();
        _repository.GetByIdAsync(deck.Id, "user-1", Arg.Any<CancellationToken>())
            .Returns(deck);

        var result = await _service.RemoveQuerySearchAsync(deck.Id, Guid.NewGuid(), CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task RemoveQuerySearchAsync_WhenDeckNotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Deck?)null);

        var result = await _service.RemoveQuerySearchAsync(Guid.NewGuid(), Guid.NewGuid(), CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task RemoveQuerySearchAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var result = await _service.RemoveQuerySearchAsync(Guid.NewGuid(), Guid.NewGuid(), CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    private static ClaimsPrincipal CreatePrincipal(string userId = "user-1") =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)]));

    private static ClaimsPrincipal CreateAnonymousPrincipal() =>
        new(new ClaimsIdentity());

    private static Deck CreateDeck() => new()
    {
        Id = Guid.NewGuid(),
        UserId = "user-1",
        Name = "Test Deck",
        Type = "Standard",
        Cards = [],
        QuerySearches = []
    };
}
