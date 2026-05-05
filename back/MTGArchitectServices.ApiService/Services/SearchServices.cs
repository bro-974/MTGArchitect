using MTGArchitect.Scryfall.Client.Services;
using MTGArchitect.Scryfall.Contracts;

namespace MTGArchitectServices.ApiService.Controllers;

public class SearchServices
{
    private readonly IScryfallClient cardSearchClient;

    public SearchServices(IScryfallClient cardSearchClient)
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
        var result = await cardSearchClient.AdvancedSearchAsync(body, cancellationToken: cancellationToken);
        return Results.Ok(new { cards = result.Cards, totalCount = result.TotalCount });
    }

    public async Task<IResult> GetCardDetail(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Results.BadRequest(new { message = "Card id is required." });

        var detail = await cardSearchClient.GetCardDetailAsync(id, cancellationToken);
        if (detail is null)
            return Results.NotFound(new { message = $"Card {id} not found." });

        var response = new CardDetailResponse(
            Id: detail.Id,
            Name: detail.Name,
            ManaCost: detail.ManaCost,
            Cmc: detail.Cmc,
            TypeLine: detail.TypeLine,
            OracleText: detail.OracleText,
            Power: detail.Power,
            Toughness: detail.Toughness,
            Loyalty: detail.Loyalty,
            Rarity: detail.Rarity,
            SetCode: detail.SetCode,
            SetName: detail.SetName,
            Legalities: detail.Legalities,
            FlavorText: detail.FlavorText,
            Artist: detail.Artist,
            ImageUrl: detail.ImageUrl,
            ImageLargeUrl: detail.ImageLargeUrl,
            Printings: detail.Printings.Select(p => new CardPrintingResponse(p.Id, p.SetCode, p.SetName, p.Rarity, p.ImageUrl)).ToList(),
            Rulings: detail.Rulings.Select(r => new CardRulingResponse(r.PublishedAt, r.Comment)).ToList());

        return Results.Ok(response);
    }
}
