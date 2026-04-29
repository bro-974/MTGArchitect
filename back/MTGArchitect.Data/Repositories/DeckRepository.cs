using Microsoft.EntityFrameworkCore;
using MTGArchitect.Data.Data;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Repositories;

internal sealed class DeckRepository(AuthDbContext dbContext) : IDeckRepository
{
    public async Task<Deck?> GetByIdWithDetailsAsync(Guid id, string userId, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Decks
            .Include(x => x.Cards)
            .Include(x => x.QuerySearches)
            .Where(x => x.Id == id && x.UserId == userId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Deck?> GetByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Decks
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Deck deck, CancellationToken cancellationToken = default)
    {
        dbContext.Decks.Add(deck);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Deck deck, Action<Deck> applyUpdates, CancellationToken cancellationToken = default)
    {
        dbContext.DeckCards.RemoveRange(deck.Cards);
        dbContext.QueryInfos.RemoveRange(deck.QuerySearches);

        applyUpdates(deck);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Deck deck, CancellationToken cancellationToken = default)
    {
        dbContext.Decks.Remove(deck);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddQuerySearchToDeckAsync(Guid deckId, QueryInfo queryInfo, CancellationToken cancellationToken = default)
    {
        queryInfo.Id = queryInfo.Id == Guid.Empty ? Guid.NewGuid() : queryInfo.Id;
        queryInfo.DeckId = deckId;

        dbContext.QueryInfos.Add(queryInfo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveQuerySearchAsync(Guid deckId, Guid queryId, CancellationToken cancellationToken = default)
    {
        var queryInfo = await dbContext.QueryInfos
            .FirstOrDefaultAsync(x => x.Id == queryId && x.DeckId == deckId, cancellationToken);

        if (queryInfo is null)
        {
            return;
        }

        dbContext.QueryInfos.Remove(queryInfo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DeckCard> AddOrIncrementCardAsync(Guid deckId, DeckCard card, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.DeckCards
            .FirstOrDefaultAsync(x => x.DeckId == deckId && x.ScryFallId == card.ScryFallId && x.IsSideBoard == card.IsSideBoard, cancellationToken);

        if (existing is not null)
        {
            existing.Quantity += card.Quantity;
            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        card.Id = Guid.NewGuid();
        card.DeckId = deckId;
        dbContext.DeckCards.Add(card);
        await dbContext.SaveChangesAsync(cancellationToken);
        return card;
    }
}
