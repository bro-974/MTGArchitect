namespace MTGArchitectServices.AuthApiService.Contracts;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, string UserId, string Email, IReadOnlyList<string> Roles);
