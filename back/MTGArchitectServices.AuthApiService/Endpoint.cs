using MTGArchitect.Data.Services;
using MTGArchitectServices.AuthApiService.Contracts;
using MTGArchitectServices.AuthApiService.Services;
using System.Security.Claims;

public static class EndpointExtensions
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Auth API service is running.");

        app.MapPost("/api/auth/register", async (
            RegisterRequest request,
            IAuthDataService authDataService) =>
        {
            var validationErrors = ValidateCredentials(request.Email, request.Password);

            if (validationErrors.Count > 0)
                return Results.ValidationProblem(validationErrors);

            var result = await authDataService.RegisterAsync(request.Email, request.Password);

            if (!result.Succeeded)
                return Results.ValidationProblem(result.Errors);

            var user = result.User!;

            return Results.Created($"/api/auth/users/{user.Id}", new
            {
                user.Id,
                user.Email
            });
        })
        .WithName("RegisterUser");

        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            IAuthDataService authDataService,
            IJwtTokenGenerator jwtTokenGenerator) =>
        {
            var validationErrors = ValidateCredentials(request.Email, request.Password);

            if (validationErrors.Count > 0)
                return Results.ValidationProblem(validationErrors);

            var user = await authDataService.AuthenticateAsync(request.Email, request.Password);

            if (user is null)
                return Results.Unauthorized();

            var token = jwtTokenGenerator.GenerateToken(user);

            return Results.Ok(new AuthResponse(
                token.AccessToken,
                token.ExpiresAtUtc,
                user.Id,
                user.Email ?? string.Empty));
        })
        .WithName("LoginUser");

        app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                userId = user.FindFirstValue(ClaimTypes.NameIdentifier),
                email = user.FindFirstValue(ClaimTypes.Email)
            });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser");

        app.MapPost("/api/auth/logout", () => Results.NoContent())
        .RequireAuthorization()
        .WithName("LogoutUser");

        return app;
    }

    private static Dictionary<string, string[]> ValidateCredentials(string email, string password)
    {
        Dictionary<string, string[]> errors = [];

        if (string.IsNullOrWhiteSpace(email))
            errors["email"] = ["Email is required."];

        if (string.IsNullOrWhiteSpace(password))
            errors["password"] = ["Password is required."];

        return errors;
    }
}
