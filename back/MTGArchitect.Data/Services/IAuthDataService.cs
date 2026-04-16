using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Services;

public interface IAuthDataService
{
    Task<RegisterUserResult> RegisterAsync(string email, string password);
    Task<ApplicationUser?> AuthenticateAsync(string email, string password);
}

public sealed record RegisterUserResult(ApplicationUser? User, Dictionary<string, string[]> Errors)
{
    public bool Succeeded => Errors.Count == 0 && User is not null;
}
