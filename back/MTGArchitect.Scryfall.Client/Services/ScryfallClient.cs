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

    public async Task<IEnumerable<Card>> AdvancedSearchAsync(CardQuerySearch body, CancellationToken cancellationToken)
    {
        var request = MappingHelpers.ToProtoRequest(body);
        var reply = await cardSearchClient.AdvanceSearchCardsAsync(request, cancellationToken: cancellationToken);
        return MappingHelpers.MapCards(reply.Cards);
    }
}

public interface IScryfallClient
{
    Task<IEnumerable<Card>> SearchCardsAsync(string query, int? pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<Card>> AdvancedSearchAsync(CardQuerySearch request, CancellationToken cancellationToken);
}