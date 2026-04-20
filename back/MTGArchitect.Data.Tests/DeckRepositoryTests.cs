using Microsoft.EntityFrameworkCore;
using MTGArchitect.Data.Data;
using MTGArchitect.Data.Models;
using MTGArchitect.Data.Repositories;

namespace MTGArchitect.Data.Tests;

[TestFixture]
public class DeckRepositoryTests
{
    private AuthDbContext _dbContext = null!;
    private IDeckRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AuthDbContext(options);
        _repository = new DeckRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task AddAsync_ShouldPersistDeckWithDetails()
    {
        var deck = CreateDeck("user-1");

        await _repository.AddAsync(deck);

        var savedDeck = await _dbContext.Decks
            .Include(x => x.Cards)
            .Include(x => x.QuerySearches)
            .SingleOrDefaultAsync(x => x.Id == deck.Id);

        Assert.That(savedDeck, Is.Not.Null);
        Assert.That(savedDeck!.Cards, Has.Count.EqualTo(1));
        Assert.That(savedDeck.QuerySearches, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetByIdWithDetailsAsync_AsNoTracking_ShouldReturnUntrackedEntity()
    {
        var deck = CreateDeck("user-1");
        _dbContext.Decks.Add(deck);
        await _dbContext.SaveChangesAsync();

        var fetched = await _repository.GetByIdWithDetailsAsync(deck.Id, "user-1", asNoTracking: true);
        fetched!.Name = "Updated In Memory";

        await _dbContext.SaveChangesAsync();

        var persisted = await _dbContext.Decks.AsNoTracking().SingleAsync(x => x.Id == deck.Id);

        Assert.That(persisted.Name, Is.EqualTo(deck.Name));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnDeckForOwnerOnly()
    {
        var deck = CreateDeck("owner-user");
        _dbContext.Decks.Add(deck);
        await _dbContext.SaveChangesAsync();

        var existing = await _repository.GetByIdAsync(deck.Id, "owner-user");
        var missing = await _repository.GetByIdAsync(deck.Id, "other-user");

        Assert.That(existing, Is.Not.Null);
        Assert.That(missing, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldReplaceCardsAndQueriesAndPersistChanges()
    {
        var deck = CreateDeck("user-1", withDetails: false);
        _dbContext.Decks.Add(deck);
        await _dbContext.SaveChangesAsync();

        var trackedDeck = await _repository.GetByIdWithDetailsAsync(deck.Id, "user-1");

        await _repository.UpdateAsync(trackedDeck!, x =>
        {
            x.Name = "Updated Deck";
            x.Type = "Commander";
            x.Note = "Updated Note";
            x.Cards =
            [
                new DeckCard
                {
                    DeckId = x.Id,
                    CardName = "Counterspell",
                    ScryFallId = "card-2",
                    Quantity = 2,
                    Type = "Instant",
                    Cost = "UU",
                    IsSideBoard = false
                }
            ];
            x.QuerySearches =
            [
                new QueryInfo
                {
                    DeckId = x.Id,
                    Query = "t:instant",
                    SearchEngine = "Scryfall"
                }
            ];
        });

        var updated = await _dbContext.Decks
            .AsNoTracking()
            .Include(x => x.Cards)
            .Include(x => x.QuerySearches)
            .SingleAsync(x => x.Id == deck.Id);

        Assert.That(updated.Name, Is.EqualTo("Updated Deck"));
        Assert.That(updated.Cards, Has.Count.EqualTo(1));
        Assert.That(updated.Cards.Single().CardName, Is.EqualTo("Counterspell"));
        Assert.That(updated.QuerySearches, Has.Count.EqualTo(1));
        Assert.That(updated.QuerySearches.Single().Query, Is.EqualTo("t:instant"));

        Assert.That(await _dbContext.DeckCards.CountAsync(), Is.EqualTo(1));
        Assert.That(await _dbContext.QueryInfos.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveDeck()
    {
        var deck = CreateDeck("user-1");
        _dbContext.Decks.Add(deck);
        await _dbContext.SaveChangesAsync();

        var trackedDeck = await _repository.GetByIdAsync(deck.Id, "user-1");
        await _repository.DeleteAsync(trackedDeck!);

        var deleted = await _dbContext.Decks.SingleOrDefaultAsync(x => x.Id == deck.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task AddQuerySearchToDeckAsync_ShouldAddQuerySearchToDeck()
    {
        var deck = CreateDeck("user-1", withDetails: false);
        _dbContext.Decks.Add(deck);
        await _dbContext.SaveChangesAsync();

        var queryInfo = new QueryInfo
        {
            Query = "t:creature",
            SearchEngine = "Scryfall"
        };

        await _repository.AddQuerySearchToDeckAsync(deck.Id, queryInfo);

        var savedQuery = await _dbContext.QueryInfos.SingleOrDefaultAsync(x => x.DeckId == deck.Id && x.Query == "t:creature");

        Assert.That(savedQuery, Is.Not.Null);
        Assert.That(savedQuery!.SearchEngine, Is.EqualTo("Scryfall"));
        Assert.That(savedQuery.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task RemoveQuerySearchAsync_ShouldRemoveQuerySearchFromDeck()
    {
        var deck = CreateDeck("user-1", withDetails: false);
        var queryInfo = new QueryInfo
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            Query = "c:u",
            SearchEngine = "Scryfall"
        };

        _dbContext.Decks.Add(deck);
        _dbContext.QueryInfos.Add(queryInfo);
        await _dbContext.SaveChangesAsync();

        await _repository.RemoveQuerySearchAsync(deck.Id, queryInfo.Id);

        var removedQuery = await _dbContext.QueryInfos.SingleOrDefaultAsync(x => x.Id == queryInfo.Id);
        Assert.That(removedQuery, Is.Null);
    }

    private static Deck CreateDeck(string userId, bool withDetails = true)
    {
        return new Deck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Sample Deck",
            Type = "Standard",
            Note = "Initial Note",
            Cards = withDetails
                ? 
                [
                    new DeckCard
                    {
                        Id = Guid.NewGuid(),
                        CardName = "Lightning Bolt",
                        ScryFallId = "card-1",
                        Quantity = 4,
                        Type = "Instant",
                        Cost = "R",
                        IsSideBoard = false
                    }
                ]
                : [],
            QuerySearches = withDetails
                ?
                [
                    new QueryInfo
                    {
                        Id = Guid.NewGuid(),
                        Query = "c:r",
                        SearchEngine = "Scryfall"
                    }
                ]
                : []
        };
    }
}
