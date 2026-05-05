namespace MTGArchitectServices.AuthApiService.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public SeedUserOptions DefaultUser { get; init; } = new();
    public SeedUserOptions AdminUser { get; init; } = new();
}

public sealed class SeedUserOptions
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}
