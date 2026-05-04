namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class HealthTests
{
    [Test]
    public async Task ServerStatus_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/server-status");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }
}
