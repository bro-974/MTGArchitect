using Grpc.Core;
using MTGArchitect.Scryfall.Service.Contracts;
using MTGArchitect.Scryfall.Service.Core;

namespace MTGArchitect.Scryfall.Service.Services;

public class ScryfallCardSearchService(
    ICardController cardController,
    ILogger<ScryfallCardSearchService> logger) : CardSearch.CardSearchBase
{
    public override async Task<SearchCardsReply> SearchCards(SearchCardsRequest request, ServerCallContext context)
    {
        var result = await cardController.SearchCards(request.Query, request.PageSize, context.CancellationToken);
        return MappingHelpers.ToReply(result);
    }

    public override async Task<SearchCardsReply> AdvanceSearchCards(AdvanceSearchCardsRequest request, ServerCallContext context)
    {
        var query = MappingHelpers.ToCardQuerySearch(request);
        var pageSize = request.PageSize > 0 ? request.PageSize : query.PageSize;
        var result = await cardController.AdvanceSearchCards(query, pageSize, context.CancellationToken);
        return MappingHelpers.ToReply(result);
    }
}