using MTGArchitect.Data.Repositories;
using System.Security.Claims;

public class UserService(IUserRepository userRepository)
{
    public async Task<IResult> GetSettingsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var user = await userRepository.GetByIdWithSettingsAndDecksAsync(userId, cancellationToken);
        if (user is null)
            return Results.NotFound();

        return Results.Ok(MappingHelpers.ToUserSettingsResponse(user));
    }

    public async Task<IResult> GetDecksAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var user = await userRepository.GetByIdWithSettingsAndDecksAsync(userId, cancellationToken);
        if (user is null)
            return Results.NotFound();

        return Results.Ok(user.DeckWorkspace
            .Select(MappingHelpers.ToDeckResponse)
            .ToArray());
    }

    private static string? GetUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
    }
}
