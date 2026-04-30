using MTGArchitect.Scryfall.Contracts;
using static MTGArchitect.Scryfall.Service.CardController;

namespace MTGArchitect.Scryfall.Service;

public interface ICardController
{
    Task<SearchCardsResult> AdvanceSearchCards(CardQuerySearch query, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<SearchCardsResult> SearchCards(string query, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<GetCardDetailResult?> GetCardDetail(string id, CancellationToken cancellationToken = default);
}
