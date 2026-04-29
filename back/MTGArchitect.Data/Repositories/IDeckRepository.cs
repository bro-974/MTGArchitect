using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Repositories;

public interface IDeckRepository
{
    Task<Deck?> GetByIdWithDetailsAsync(Guid id, string userId, bool asNoTracking = false, CancellationToken cancellationToken = default);
    Task<Deck?> GetByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Deck deck, CancellationToken cancellationToken = default);
    Task UpdateAsync(Deck deck, Action<Deck> applyUpdates, CancellationToken cancellationToken = default);
    Task DeleteAsync(Deck deck, CancellationToken cancellationToken = default);
    Task AddQuerySearchToDeckAsync(Guid deckId, QueryInfo queryInfo, CancellationToken cancellationToken = default);
    Task RemoveQuerySearchAsync(Guid deckId, Guid queryId, CancellationToken cancellationToken = default);
    Task<DeckCard> AddOrIncrementCardAsync(Guid deckId, DeckCard card, CancellationToken cancellationToken = default);
}
