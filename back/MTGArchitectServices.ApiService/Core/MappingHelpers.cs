using MTGArchitect.Data.Models;

public static class MappingHelpers
{
    public static UserSettingsResponse ToUserSettingsResponse(ApplicationUser user)
    {
        return new UserSettingsResponse(
            user.DisplayName,
            user.Language,
            user.Theme);
    }

    public static DeckResponse ToDeckResponse(Deck deck)
    {
        return new DeckResponse(
            deck.Id,
            deck.Name,
            deck.Type,
            deck.Note,
            deck.QuerySearches.Select(ToQueryInfoResponse).ToArray(),
            deck.Cards.Select(ToDeckCardResponse).ToArray());
    }

    public static QueryInfoResponse ToQueryInfoResponse(QueryInfo queryInfo)
    {
        return new QueryInfoResponse(
            queryInfo.Id,
            queryInfo.Query,
            queryInfo.SearchEngine);
    }

    public static DeckCardResponse ToDeckCardResponse(DeckCard card)
    {
        return new DeckCardResponse(
            card.Id,
            card.CardName,
            card.ScryFallId,
            card.Quantity,
            card.Type,
            card.Cost,
            card.IsSideBoard);
    }

    public static Deck ToDeck(DeckUpsertRequest request, string userId)
    {
        return new Deck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Type = request.Type,
            Note = request.Note,
            QuerySearches = ToQueryInfos(request.QuerySearches),
            Cards = ToDeckCards(request.Cards)
        };
    }

    public static void Apply(this Deck deck, DeckUpsertRequest request)
    {
        deck.Name = request.Name;
        deck.Type = request.Type;
        deck.Note = request.Note;
        deck.QuerySearches = ToQueryInfos(request.QuerySearches);
        deck.Cards = ToDeckCards(request.Cards);
    }

    public static List<QueryInfo> ToQueryInfos(List<QueryInfoUpsertRequest>? queryInfos)
    {
        return (queryInfos ?? [])
            .Select(x => new QueryInfo
            {
                Id = x.Id.GetValueOrDefault() == Guid.Empty ? Guid.NewGuid() : x.Id.Value,
                Query = x.QueryJson,
                SearchEngine = x.SearchEngine
            })
            .ToList();
    }

    public static List<DeckCard> ToDeckCards(List<DeckCardUpsertRequest>? cards)
    {
        return (cards ?? [])
            .Select(x => new DeckCard
            {
                Id = Guid.NewGuid(),
                CardName = x.CardName,
                ScryFallId = x.ScryFallId,
                Quantity = x.Quantity,
                Type = x.Type,
                Cost = x.Cost,
                IsSideBoard = x.IsSideBoard
            })
            .ToList();
    }
}
