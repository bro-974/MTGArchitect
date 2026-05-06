using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace MTGArchitect.Api.Integration.Tests;

[SetUpFixture]
public class IntegrationTestFixture
{
    private static DistributedApplication _app = null!;

    public static HttpClient ApiClient { get; private set; } = null!;
    public static HttpClient AnonClient { get; private set; } = null!;
    public static string UserId { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        const string testEmail = "admin@admin.fr";
        const string testPassword = "a123456798!";

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MTGArchitect_AppHost>();
        appHost.Configuration["Jwt:Key"] = "integration-test-signing-key-32chars!!";

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        await _app.ResourceNotifications.WaitForResourceHealthyAsync("authapiservice");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice");

        using var authClient = _app.CreateHttpClient("authapiservice");
        var loginResponse = await authClient.PostAsJsonAsync("/api/auth/login", new
        {
            email = testEmail,
            password = testPassword
        });
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        UserId = auth!.UserId;

        ApiClient = _app.CreateHttpClient("apiservice");
        ApiClient.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);

        AnonClient = _app.CreateHttpClient("apiservice");
    }

    [OneTimeTearDown]
    public async Task StopAsync()
    {
        ApiClient.Dispose();
        AnonClient.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    private sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, string UserId, string Email);
}
