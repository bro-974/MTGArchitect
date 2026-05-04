namespace MTGArchitect.Api.Integration.Tests;

[TestFixture]
public class HealthTests
{
    [Test]
    public async Task GetServerStatus_ReturnsOk()
    {
        var response = await IntegrationTestFixture.ApiClient.GetAsync("/api/server-status");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }
}
