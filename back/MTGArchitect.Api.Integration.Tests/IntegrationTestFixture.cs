using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace MTGArchitect.Api.Integration.Tests;

[SetUpFixture]
public class IntegrationTestFixture
{
    private static DistributedApplication? _app;

    public static HttpClient ApiClient { get; private set; } = null!;
    public static HttpClient AnonClient { get; private set; } = null!;
    public static string BearerToken { get; private set; } = string.Empty;
    public static string UserId { get; private set; } = string.Empty;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MTGArchitect_AppHost>();

        appHost.Configuration["Jwt:Key"] = "integration-test-only-secret-key-32chars!!";

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        await _app.ResourceNotifications.WaitForResourceHealthyAsync("authapiservice");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice");

        using var loginClient = _app.CreateHttpClient("authapiservice");
        var loginResponse = await loginClient.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nordyn@hotmail.fr",
            password = "Aqw1!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        BearerToken = loginResult!.AccessToken;
        UserId = loginResult.UserId;

        ApiClient = _app.CreateHttpClient("apiservice", "api-http");
        ApiClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BearerToken);

        AnonClient = _app.CreateHttpClient("apiservice", "api-http");
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        ApiClient?.Dispose();
        AnonClient?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    private record LoginResult(string AccessToken, DateTime ExpiresAtUtc, string UserId, string Email);
}
