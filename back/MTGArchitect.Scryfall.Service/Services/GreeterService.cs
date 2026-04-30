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

    public override async Task<CardDetailReply> GetCardDetail(GetCardDetailRequest request, ServerCallContext context)
    {
        var result = await cardController.GetCardDetail(request.Id, context.CancellationToken);
        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Card {request.Id} not found"));

        var reply = new CardDetailReply
        {
            Id = result.Id,
            Name = result.Name,
            ManaCost = result.ManaCost,
            Cmc = result.Cmc,
            TypeLine = result.TypeLine,
            OracleText = result.OracleText,
            Power = result.Power,
            Toughness = result.Toughness,
            Loyalty = result.Loyalty,
            Rarity = result.Rarity,
            SetCode = result.SetCode,
            SetName = result.SetName,
            FlavorText = result.FlavorText,
            Artist = result.Artist,
            ImageUrl = result.ImageUrl,
            ImageLargeUrl = result.ImageLargeUrl
        };

        foreach (var (format, legality) in result.Legalities)
            reply.Legalities[format] = legality;

        reply.Printings.AddRange(result.Printings.Select(p => new CardPrintingItem
        {
            Id = p.Id,
            SetCode = p.SetCode,
            SetName = p.SetName,
            Rarity = p.Rarity,
            ImageUrl = p.ImageUrl
        }));

        reply.Rulings.AddRange(result.Rulings.Select(r => new CardRulingItem
        {
            PublishedAt = r.PublishedAt,
            Comment = r.Comment
        }));

        return reply;
    }
}