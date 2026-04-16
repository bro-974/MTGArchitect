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

public sealed record QueryInfoUpsertRequest(
    Guid? Id,
    string Query,
    string SearchEngine);

public sealed record DeckCardUpsertRequest(
    string CardName,
    string ScryFallId,
    int Quantity,
    string Type,
    string? Cost,
    bool IsSideBoard);
