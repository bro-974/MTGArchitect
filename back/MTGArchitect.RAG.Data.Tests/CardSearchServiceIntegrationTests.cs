using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MTGArchitect.RAG.Data.Data;
using MTGArchitect.RAG.Data.Repositories;
using MTGArchitect.RAG.Data.Services;
using NUnit.Framework;

namespace MTGArchitect.RAG.Data.Tests;

[TestFixture]
public class CardSearchServiceIntegrationTests
{
    private RagDbContext _db = null!;
    private CardSearchService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("ragdb")
            ?? throw new InvalidOperationException("ragdb connection string missing.");
        var lmStudioUri = config["LmStudioUri"]
            ?? throw new InvalidOperationException("LmStudioUri missing.");

        var options = new DbContextOptionsBuilder<RagDbContext>()
            .UseNpgsql(connectionString, o => o.UseVector())
            .Options;

        _db = new RagDbContext(options);

        var repository = new CardEmbeddingRepository(_db, NullLogger<CardEmbeddingRepository>.Instance);
        var http = new HttpClient();
        var embeddingService = new EmbeddingService(http, lmStudioUri);

        _sut = new CardSearchService(repository, embeddingService, config, NullLogger<CardSearchService>.Instance);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    [Ignore("Requires actual database and LM Studio setup.")]
    public async Task GetSimilarCardsAsync_ExactCardName_ReturnsItFirst()
    {
        const string cardName = "Llanowar Elves";

        var results = await _sut.GetSimilarCardsAsync(cardName, limit: 5);

        Assert.That(results, Is.Not.Empty, "Expected at least one result.");
        Assert.That(results[0].Name, Is.EqualTo(cardName),
            $"Expected '{cardName}' as first result but got '{results[0].Name}'.");
    }
}
