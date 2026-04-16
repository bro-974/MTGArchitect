using Microsoft.EntityFrameworkCore;
using MTGArchitect.Data.Data;
using System.Security.Claims;

public class DeckService(AuthDbContext dbContext)
{
    public async Task<IResult> GetByIdAsync(Guid id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await dbContext.Decks
            .AsNoTracking()
            .Include(x => x.Cards)
            .Include(x => x.QuerySearches)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

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

        dbContext.Decks.Add(deck);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/deck/{deck.Id}", MappingHelpers.ToDeckResponse(deck));
    }

    public async Task<IResult> UpdateAsync(Guid id, DeckUpsertRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await dbContext.Decks
            .Include(x => x.Cards)
            .Include(x => x.QuerySearches)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (deck is null)
            return Results.NotFound();

        dbContext.DeckCards.RemoveRange(deck.Cards);
        dbContext.QueryInfos.RemoveRange(deck.QuerySearches);
        deck.Apply(request);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(MappingHelpers.ToDeckResponse(deck));
    }

    public async Task<IResult> DeleteAsync(Guid id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var deck = await dbContext.Decks
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (deck is null)
            return Results.NotFound();

        dbContext.Decks.Remove(deck);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static string? GetUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
    }
}
