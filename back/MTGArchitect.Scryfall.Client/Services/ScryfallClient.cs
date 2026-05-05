using Grpc.Core;
using MTGArchitect.Scryfall.Contracts;
using MTGArchitect.Scryfall.Service.Contracts;
using MTGArchitectServices.Scryfall.Client;

namespace MTGArchitect.Scryfall.Client.Services;

public class ScryfallClient: IScryfallClient
{

    private readonly CardSearch.CardSearchClient cardSearchClient;

    public ScryfallClient(CardSearch.CardSearchClient cardSearchClient)
    {
        this.cardSearchClient = cardSearchClient;
    }

    public async Task<IEnumerable<Card>> SearchCardsAsync(string query, int? pageSize, CancellationToken cancellationToken)
    {
        var reply = await cardSearchClient.SearchCardsAsync(new SearchCardsRequest
        {
            Query = query,
            PageSize = pageSize.GetValueOrDefault(20)
        }, cancellationToken: cancellationToken);

        return MappingHelpers.MapCards(reply.Cards);
    }

    public async Task<CardSearchResult> AdvancedSearchAsync(CardQuerySearch body, CancellationToken cancellationToken)
    {
        var request = MappingHelpers.ToProtoRequest(body);
        var reply = await cardSearchClient.AdvanceSearchCardsAsync(request, cancellationToken: cancellationToken);
        return new CardSearchResult
        {
            Cards = MappingHelpers.MapCards(reply.Cards).ToList(),
            TotalCount = reply.TotalCount
        };
    }

    public async Task<CardDetail?> GetCardDetailAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var reply = await cardSearchClient.GetCardDetailAsync(
                new GetCardDetailRequest { Id = id },
                cancellationToken: cancellationToken);

            return new CardDetail
            {
                Id = reply.Id,
                Name = reply.Name,
                ManaCost = reply.ManaCost,
                Cmc = reply.Cmc,
                TypeLine = reply.TypeLine,
                OracleText = reply.OracleText,
                Power = reply.Power,
                Toughness = reply.Toughness,
                Loyalty = reply.Loyalty,
                Rarity = reply.Rarity,
                SetCode = reply.SetCode,
                SetName = reply.SetName,
                Legalities = new Dictionary<string, string>(reply.Legalities),
                FlavorText = reply.FlavorText,
                Artist = reply.Artist,
                ImageUrl = reply.ImageUrl,
                ImageLargeUrl = reply.ImageLargeUrl,
                Printings = reply.Printings.Select(p => new Contracts.CardPrinting
                {
                    Id = p.Id,
                    SetCode = p.SetCode,
                    SetName = p.SetName,
                    Rarity = p.Rarity,
                    ImageUrl = p.ImageUrl
                }).ToList(),
                Rulings = reply.Rulings.Select(r => new Contracts.CardRuling
                {
                    PublishedAt = r.PublishedAt,
                    Comment = r.Comment
                }).ToList()
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}

public interface IScryfallClient
{
    Task<IEnumerable<Card>> SearchCardsAsync(string query, int? pageSize, CancellationToken cancellationToken);
    Task<CardSearchResult> AdvancedSearchAsync(CardQuerySearch request, CancellationToken cancellationToken);
    Task<CardDetail?> GetCardDetailAsync(string id, CancellationToken cancellationToken);
}