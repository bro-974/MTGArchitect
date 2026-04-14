using MTGArchitect.Scryfall.Contracts;
using MTGArchitect.Scryfall.Service.Core;
using MTGArchitect.Scryfall.Service.Models;
using System.Text.Json;

namespace MTGArchitect.Scryfall.Service;

public class CardController : ICardController
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<CardController> logger;

    public CardController(IHttpClientFactory httpClientFactory,
    ILogger<CardController> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public record SearchCardsResult(List<CardItem> Cards);

    public async Task<SearchCardsResult> AdvanceSearchCards(CardQuerySearch query, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var search = new CardQueryBuilder().SetFromQuery(query).Build();
        if (string.IsNullOrWhiteSpace(search))
            search = "type:creature";
        var resolvedPageSize = query.PageSize > 0 ? query.PageSize : pageSize;
        return await Search(search, resolvedPageSize, cancellationToken);
    }

    public async Task<SearchCardsResult> SearchCards(string query, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchQuery = string.IsNullOrWhiteSpace(query) ? "type:creature" : query.Trim();
        return await Search(searchQuery, pageSize, cancellationToken);
    }


    private async Task<SearchCardsResult> Search(string searchQuery, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var validatedPageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 50);
        var apiQuery = Uri.EscapeDataString(searchQuery);
        var apiPath = $"cards/search?q={apiQuery}&unique=cards&order=name";
        var client = httpClientFactory.CreateClient("scryfall");

        try
        {
            using var response = await client.GetAsync(apiPath, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var cards = document.RootElement.TryGetProperty("data", out var dataElement)
                && dataElement.ValueKind == JsonValueKind.Array
                ? dataElement.EnumerateArray().Take(validatedPageSize).Select(MapCard).ToList()
                : [];

            return new SearchCardsResult(cards);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(exception, "Scryfall search failed for query {Query}", searchQuery);
            return new SearchCardsResult([]);
        }
    }

    private static CardItem MapCard(JsonElement source)
    {
        return new CardItem(
            Id: GetString(source, "id"),
            Name: GetString(source, "name"),
            ManaCost: GetString(source, "mana_cost"),
            TypeLine: GetString(source, "type_line"),
            SetCode: GetString(source, "set"),
            ImageUrl: GetImageUrl(source)
        );
    }

    private static string GetImageUrl(JsonElement source)
    {
        if (source.TryGetProperty("image_uris", out var imageUris)
            && imageUris.ValueKind == JsonValueKind.Object
            && imageUris.TryGetProperty("normal", out var normalImage)
            && normalImage.ValueKind == JsonValueKind.String)
        {
            return normalImage.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static string GetString(JsonElement source, string name)
    {
        return source.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }
}
