using MTGArchitect.Scryfall.Client.Services;
using MTGArchitect.Scryfall.Contracts;

namespace MTGArchitectServices.ApiService.Controllers;

public class SearchController
{
    private readonly IScryfallClient cardSearchClient;

    public SearchController(IScryfallClient cardSearchClient)
    {
        this.cardSearchClient = cardSearchClient;
    }

    public async Task<IResult> QuerySearch(string q, int? pageSize, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Results.BadRequest(new { message = "Query parameter q is required." });

        var reply = await cardSearchClient.SearchCardsAsync(q, pageSize.GetValueOrDefault(20),
         cancellationToken: cancellationToken);

        return Results.Ok(reply);
    }

    public async Task<IResult> AdvancedSearch(CardQuerySearch body, CancellationToken cancellationToken)
    {
        var reply = await cardSearchClient.AdvancedSearchAsync(body, cancellationToken: cancellationToken);
        return Results.Ok(reply);
    }
}
