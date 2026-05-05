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

    public record SearchCardsResult(List<CardItem> Cards, int TotalCount);

    public record CardPrinting(string Id, string SetCode, string SetName, string Rarity, string ImageUrl);
    public record CardRuling(string PublishedAt, string Comment);
    public record GetCardDetailResult(
        string Id, string Name, string ManaCost, float Cmc,
        string TypeLine, string OracleText, string Power, string Toughness, string Loyalty,
        string Rarity, string SetCode, string SetName,
        Dictionary<string, string> Legalities, string FlavorText, string Artist,
        string ImageUrl, string ImageLargeUrl,
        List<CardPrinting> Printings, List<CardRuling> Rulings);

    public async Task<SearchCardsResult> AdvanceSearchCards(CardQuerySearch query, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var search = new CardQueryBuilder().SetFromQuery(query).Build();
        if (string.IsNullOrWhiteSpace(search))
            search = "type:creature";
        var resolvedPageSize = query.PageSize is > 0 ? query.PageSize.Value : pageSize;
        var resolvedPage = query.Page ?? 0;
        return await Search(search, resolvedPageSize, resolvedPage, cancellationToken);
    }

    public async Task<SearchCardsResult> SearchCards(string query, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchQuery = string.IsNullOrWhiteSpace(query) ? "type:creature" : query.Trim();
        return await Search(searchQuery, pageSize, 0, cancellationToken);
    }


    public async Task<GetCardDetailResult?> GetCardDetail(string id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("scryfall");

        try
        {
            using var cardResponse = await client.GetAsync($"cards/{id}", cancellationToken);
            if (cardResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            cardResponse.EnsureSuccessStatusCode();

            await using var cardStream = await cardResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var cardDoc = await JsonDocument.ParseAsync(cardStream, cancellationToken: cancellationToken);
            var root = cardDoc.RootElement;

            var printsUri = GetString(root, "prints_search_uri");
            var rulingsUri = GetString(root, "rulings_uri");

            var printingsTask = FetchPrintings(client, printsUri, cancellationToken);
            var rulingsTask = FetchRulings(client, rulingsUri, cancellationToken);
            await Task.WhenAll(printingsTask, rulingsTask);

            var legalities = new Dictionary<string, string>();
            if (root.TryGetProperty("legalities", out var legalitiesEl) && legalitiesEl.ValueKind == JsonValueKind.Object)
                foreach (var prop in legalitiesEl.EnumerateObject())
                    legalities[prop.Name] = prop.Value.GetString() ?? string.Empty;

            return new GetCardDetailResult(
                Id: GetString(root, "id"),
                Name: GetString(root, "name"),
                ManaCost: GetString(root, "mana_cost"),
                Cmc: root.TryGetProperty("cmc", out var cmcEl) ? (float)cmcEl.GetDouble() : 0f,
                TypeLine: GetString(root, "type_line"),
                OracleText: GetString(root, "oracle_text"),
                Power: GetString(root, "power"),
                Toughness: GetString(root, "toughness"),
                Loyalty: GetString(root, "loyalty"),
                Rarity: GetString(root, "rarity"),
                SetCode: GetString(root, "set"),
                SetName: GetString(root, "set_name"),
                Legalities: legalities,
                FlavorText: GetString(root, "flavor_text"),
                Artist: GetString(root, "artist"),
                ImageUrl: GetImageUrl(root),
                ImageLargeUrl: GetLargeImageUrl(root),
                Printings: await printingsTask,
                Rulings: await rulingsTask);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(exception, "Scryfall card detail fetch failed for id {Id}", id);
            return null;
        }
    }

    private static async Task<List<CardPrinting>> FetchPrintings(HttpClient client, string uri, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uri)) return [];
        try
        {
            using var response = await client.GetAsync(uri + "&order=released&dir=desc&unique=prints", cancellationToken);
            if (!response.IsSuccessStatusCode) return [];
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                return [];
            return data.EnumerateArray().Take(20).Select(p => new CardPrinting(
                Id: GetString(p, "id"),
                SetCode: GetString(p, "set"),
                SetName: GetString(p, "set_name"),
                Rarity: GetString(p, "rarity"),
                ImageUrl: GetImageUrl(p))).ToList();
        }
        catch { return []; }
    }

    private static async Task<List<CardRuling>> FetchRulings(HttpClient client, string uri, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uri)) return [];
        try
        {
            using var response = await client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return [];
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                return [];
            return data.EnumerateArray().Select(r => new CardRuling(
                PublishedAt: GetString(r, "published_at"),
                Comment: GetString(r, "comment"))).ToList();
        }
        catch { return []; }
    }

    private static string GetLargeImageUrl(JsonElement source)
    {
        if (source.TryGetProperty("image_uris", out var imageUris)
            && imageUris.ValueKind == JsonValueKind.Object
            && imageUris.TryGetProperty("large", out var largeImage)
            && largeImage.ValueKind == JsonValueKind.String)
        {
            return largeImage.GetString() ?? string.Empty;
        }
        return string.Empty;
    }

    private async Task<SearchCardsResult> Search(string searchQuery, int pageSize, int page, CancellationToken cancellationToken)
    {
        const int scryfallPageSize = 175;
        var validatedPageSize = Math.Clamp(pageSize, 1, 50);
        var offset = page * validatedPageSize;
        var scryfallPage = offset / scryfallPageSize + 1;
        var skipWithin = offset % scryfallPageSize;
        var client = httpClientFactory.CreateClient("scryfall");

        try
        {
            var (cards, totalCount, hasMore) = await FetchScryfallPage(client, searchQuery, scryfallPage, cancellationToken);
            var result = cards.Skip(skipWithin).Take(validatedPageSize).ToList();

            if (result.Count < validatedPageSize && hasMore)
            {
                var needed = validatedPageSize - result.Count;
                var (nextCards, _, _) = await FetchScryfallPage(client, searchQuery, scryfallPage + 1, cancellationToken);
                result.AddRange(nextCards.Take(needed));
            }

            return new SearchCardsResult(result, totalCount);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(exception, "Scryfall search failed for query {Query}", searchQuery);
            return new SearchCardsResult([], 0);
        }
    }

    private async Task<(List<CardItem> Cards, int TotalCount, bool HasMore)> FetchScryfallPage(
        HttpClient client, string searchQuery, int scryfallPage, CancellationToken cancellationToken)
    {
        var apiQuery = Uri.EscapeDataString(searchQuery);
        var apiPath = $"cards/search?q={apiQuery}&unique=cards&order=name&page={scryfallPage}";

        using var response = await client.GetAsync(apiPath, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return ([], 0, false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var totalCount = document.RootElement.TryGetProperty("total_cards", out var totalEl)
            && totalEl.ValueKind == JsonValueKind.Number ? totalEl.GetInt32() : 0;
        var hasMore = document.RootElement.TryGetProperty("has_more", out var hasMoreEl)
            && hasMoreEl.ValueKind == JsonValueKind.True;
        var cards = document.RootElement.TryGetProperty("data", out var dataElement)
            && dataElement.ValueKind == JsonValueKind.Array
            ? dataElement.EnumerateArray().Select(MapCard).ToList()
            : [];

        return (cards, totalCount, hasMore);
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
