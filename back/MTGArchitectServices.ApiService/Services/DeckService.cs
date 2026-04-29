using MTGArchitect.Data.Models;
using MTGArchitect.Data.Repositories;
using System.Security.Claims;

public class DeckService(IDeckRepository deckRepository)
{
    public async Task<IResult> GetByIdAsync(Guid id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await deckRepository.GetByIdWithDetailsAsync(id, userId, asNoTracking: true, cancellationToken);

        return deck is null
            ? Results.NotFound()
            : Results.Ok(MappingHelpers.ToDeckResponse(deck));
    }

    public async Task<IResult> CreateAsync(DeckUpsertRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = MappingHelpers.ToDeck(request, userId);

        await deckRepository.AddAsync(deck, cancellationToken);

        return Results.Created($"/api/deck/{deck.Id}", MappingHelpers.ToDeckResponse(deck));
    }

    public async Task<IResult> UpdateAsync(Guid id, DeckUpsertRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await deckRepository.GetByIdWithDetailsAsync(id, userId, cancellationToken: cancellationToken);

        if (deck is null)
            return Results.NotFound();

        await deckRepository.UpdateAsync(deck, x => x.Apply(request), cancellationToken);

        return Results.Ok(MappingHelpers.ToDeckResponse(deck));
    }

    public async Task<IResult> DeleteAsync(Guid id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await deckRepository.GetByIdAsync(id, userId, cancellationToken);

        if (deck is null)
            return Results.NotFound();

        await deckRepository.DeleteAsync(deck, cancellationToken);

        return Results.NoContent();
    }

    public async Task<IResult> AddQuerySearchToDeckAsync(Guid deckId, QueryInfoUpsertRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await deckRepository.GetByIdAsync(deckId, userId, cancellationToken);
        if (deck is null)
            return Results.NotFound();

        var queryInfo = new QueryInfo
        {
            Id = request.Id.GetValueOrDefault(),
            Query = request.QueryJson,
            SearchEngine = request.SearchEngine
        };

        await deckRepository.AddQuerySearchToDeckAsync(deckId, queryInfo, cancellationToken);

        return Results.Created($"/api/deck/{deckId}/query-search/{queryInfo.Id}", MappingHelpers.ToQueryInfoResponse(queryInfo));
    }

    public async Task<IResult> AddCardToDeckAsync(Guid deckId, DeckCardAddRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await deckRepository.GetByIdAsync(deckId, userId, cancellationToken);
        if (deck is null)
            return Results.NotFound();

        var card = new DeckCard
        {
            CardName = request.CardName,
            ScryFallId = request.ScryFallId,
            Quantity = request.Quantity,
            Type = request.Type,
            Cost = request.Cost,
            IsSideBoard = request.IsSideBoard
        };

        var result = await deckRepository.AddOrIncrementCardAsync(deckId, card, cancellationToken);
        return Results.Ok(MappingHelpers.ToDeckCardResponse(result));
    }

    public async Task<IResult> RemoveQuerySearchAsync(Guid deckId, Guid queryId, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await deckRepository.GetByIdAsync(deckId, userId, cancellationToken);
        if (deck is null)
            return Results.NotFound();

        await deckRepository.RemoveQuerySearchAsync(deckId, queryId, cancellationToken);

        return Results.NoContent();
    }

    private static string? GetUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
    }
}
