using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MTGArchitect.Scryfall.Service.Tests;

[TestFixture]
[Category("Integration")]
public class CardSearchIntegrationTests
{
    private ServiceProvider _serviceProvider = null!;
    private CardController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole());

        services.AddHttpClient("scryfall", client =>
        {
            client.BaseAddress = new Uri("https://api.scryfall.com/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MTGArchitect/1.0");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
        });

        _serviceProvider = services.BuildServiceProvider();

        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        var logger = _serviceProvider.GetRequiredService<ILogger<CardController>>();

        _controller = new CardController(httpClientFactory, logger);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [Test]
    public async Task CanCallRealApi_SearchForBlackLotus_ReturnsResults()
    {
        var result = await _controller.SearchCards("Black Lotus", 10);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Cards, Is.Not.Empty, "Should return at least one card");
            Assert.That(result.Cards.Any(c => c.Name.Contains("Black Lotus", StringComparison.OrdinalIgnoreCase)), 
                Is.True, "Should contain a card with 'Black Lotus' in the name");

            var firstCard = result.Cards[0];
            Assert.That(firstCard.Id, Is.Not.Empty, "Card should have an ID");
            Assert.That(firstCard.Name, Is.Not.Empty, "Card should have a name");
            Assert.That(firstCard.SetCode, Is.Not.Empty, "Card should have a set code");
        });
    }

    [Test]
    public async Task CanCallRealApi_SearchForCreatures_ReturnsResults()
    {
        var result = await _controller.SearchCards("type:creature power=1", 5);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Is.Not.Empty);
            Assert.That(result.Cards, Has.Count.LessThanOrEqualTo(5), "Should respect page size limit");
            Assert.That(result.Cards.All(c => c.TypeLine.Contains("Creature")), 
                Is.True, "All returned cards should be creatures");
        });
    }

    [Test]
    public async Task CanCallRealApi_WithInvalidQuery_ReturnsEmptyResult()
    {
        var result = await _controller.SearchCards("thisdoesnotexistinmtg12345xyz", 10);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Is.Empty, "Invalid query should return empty results");
        });
    }

    [Test]
    public async Task CanCallRealApi_WithEmptyQuery_UsesDefaultAndReturnsCreatures()
    {
        var result = await _controller.SearchCards("", 5);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Is.Not.Empty, "Default query should return results");
            Assert.That(result.Cards.All(c => c.TypeLine.Contains("Creature")), 
                Is.True, "Default query should return creatures");
        });
    }
}
