public sealed record UserSettingsResponse(string? DisplayName, string? Language, string? Theme);

public sealed record DeckResponse(
    Guid Id,
    string Name,
    string Type,
    string? Note,
    IReadOnlyCollection<QueryInfoResponse> QuerySearches,
    IReadOnlyCollection<DeckCardResponse> Cards);

public sealed record QueryInfoResponse(
    Guid Id,
    string Query,
    string SearchEngine);

public sealed record DeckCardResponse(
    Guid Id,
    string CardName,
    string ScryFallId,
    int Quantity,
    string Type,
    string? Cost,
    bool IsSideBoard);

public sealed record DeckUpsertRequest(
    string Name,
    string Type,
    string? Note,
    List<QueryInfoUpsertRequest>? QuerySearches,
    List<DeckCardUpsertRequest>? Cards);

/// <summary>
/// Request to add or update a query search for a deck.
/// </summary>
/// <param name="Id">Optional query ID. If null or empty, a new ID will be generated.</param>
/// <param name="QueryJson">Serialized JSON string representing a CardQuerySearch object, containing all filter parameters for card search. This allows frontend to serialize the query form and reuse it with the advanced search API.</param>
/// <param name="SearchEngine">Search engine identifier (e.g., "Scryfall").</param>
public sealed record QueryInfoUpsertRequest(
    Guid? Id,
    string QueryJson,
    string SearchEngine);

public sealed record DeckCardUpsertRequest(
    string CardName,
    string ScryFallId,
    int Quantity,
    string Type,
    string? Cost,
    bool IsSideBoard);

/// <summary>
/// Request to add or increment a card in a deck. Quantity is the delta to add (not the final value).
/// </summary>
public sealed record DeckCardAddRequest(
    string CardName,
    string ScryFallId,
    int Quantity,
    string Type,
    string? Cost,
    bool IsSideBoard);

public sealed record CardDetailResponse(
    string Id,
    string Name,
    string ManaCost,
    float Cmc,
    string TypeLine,
    string OracleText,
    string Power,
    string Toughness,
    string Loyalty,
    string Rarity,
    string SetCode,
    string SetName,
    Dictionary<string, string> Legalities,
    string FlavorText,
    string Artist,
    string ImageUrl,
    string ImageLargeUrl,
    IReadOnlyList<CardPrintingResponse> Printings,
    IReadOnlyList<CardRulingResponse> Rulings);

public sealed record CardPrintingResponse(string Id, string SetCode, string SetName, string Rarity, string ImageUrl);
public sealed record CardRulingResponse(string PublishedAt, string Comment);
