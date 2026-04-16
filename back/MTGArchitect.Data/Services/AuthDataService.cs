using Microsoft.AspNetCore.Identity;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Services;

internal sealed class AuthDataService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IAuthDataService
{
    public async Task<RegisterUserResult> RegisterAsync(string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
            return new RegisterUserResult(user, []);

        return new RegisterUserResult(null, ToValidationProblem(result.Errors));
    }

    public async Task<ApplicationUser?> AuthenticateAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || user.IsDeleted)
            return null;

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

        return signInResult.Succeeded ? user : null;
    }

    private static Dictionary<string, string[]> ToValidationProblem(IEnumerable<IdentityError> errors)
    {
        return errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());
    }
}
