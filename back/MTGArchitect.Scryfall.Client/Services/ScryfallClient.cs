using MTGArchitect.Scryfall.Service.Contracts;

namespace MTGArchitect.Scryfall.Client.Services;

public class ScryfallClient
{

    private readonly CardSearch.CardSearchClient cardSearchClient;

    public ScryfallClient(CardSearch.CardSearchClient cardSearchClient)
    {
        this.cardSearchClient = cardSearchClient;
    }

    public async Task<SearchCardsReply> SearchCardsAsync(string query, int? pageSize, CancellationToken cancellationToken)
    {
        var reply = await cardSearchClient.SearchCardsAsync(new SearchCardsRequest
        {
            Query = query,
            PageSize = pageSize.GetValueOrDefault(20)
        }, cancellationToken: cancellationToken);

        return reply;
    }
}

