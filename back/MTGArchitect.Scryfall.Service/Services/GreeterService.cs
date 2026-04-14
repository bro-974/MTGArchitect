using Grpc.Core;
using MTGArchitect.Scryfall.Service.Contracts;

namespace MTGArchitect.Scryfall.Service.Services;

public class ScryfallCardSearchService(
    CardController cardController,
    ILogger<ScryfallCardSearchService> logger) : CardSearch.CardSearchBase
{
    public override async Task<SearchCardsReply> SearchCards(SearchCardsRequest request, ServerCallContext context)
    {
        var result = await cardController.SearchCards(request.Query, request.PageSize, context.CancellationToken);

        var reply = new SearchCardsReply();
        reply.Cards.AddRange(result.Cards.Select(MapToGrpcCard));
        return reply;
    }

    private static CardItem MapToGrpcCard(CardController.CardItem card)
    {
        return new CardItem
        {
            Id = card.Id,
            Name = card.Name,
            ManaCost = card.ManaCost,
            TypeLine = card.TypeLine,
            SetCode = card.SetCode,
            ImageUrl = card.ImageUrl
        };
    }
}
