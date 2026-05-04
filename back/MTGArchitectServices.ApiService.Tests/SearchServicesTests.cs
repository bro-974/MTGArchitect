using Microsoft.AspNetCore.Http;
using MTGArchitect.Scryfall.Client.Services;
using MTGArchitect.Scryfall.Contracts;
using MTGArchitectServices.ApiService.Controllers;
using NSubstitute;

namespace MTGArchitectServices.ApiService.Tests;

[TestFixture]
public class SearchServicesTests
{
    private IScryfallClient _client = null!;
    private SearchServices _service = null!;

    [SetUp]
    public void Setup()
    {
        _client = Substitute.For<IScryfallClient>();
        _service = new SearchServices(_client);
    }

    [Test]
    public async Task QuerySearch_WithValidQuery_ReturnsOk()
    {
        _client.SearchCardsAsync("bolt", Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns([new Card { Name = "Lightning Bolt" }]);

        var result = await _service.QuerySearch("bolt", null, default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task QuerySearch_WithEmptyQuery_ReturnsBadRequest()
    {
        var result = await _service.QuerySearch("  ", null, default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(400));
        await _client.DidNotReceive().SearchCardsAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task AdvancedSearch_WithValidBody_ReturnsOk()
    {
        var body = new CardQuerySearch();
        _client.AdvancedSearchAsync(body, Arg.Any<CancellationToken>())
            .Returns([new Card { Name = "Counterspell" }]);

        var result = await _service.AdvancedSearch(body, default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetCardDetail_WhenCardExists_ReturnsOk()
    {
        var detail = new CardDetail { Id = "abc", Name = "Counterspell" };
        _client.GetCardDetailAsync("abc", Arg.Any<CancellationToken>())
            .Returns(detail);

        var result = await _service.GetCardDetail("abc", default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetCardDetail_WhenCardNotFound_ReturnsNotFound()
    {
        _client.GetCardDetailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((CardDetail?)null);

        var result = await _service.GetCardDetail("unknown-id", default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetCardDetail_WithEmptyId_ReturnsBadRequest()
    {
        var result = await _service.GetCardDetail("  ", default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(400));
        await _client.DidNotReceive().GetCardDetailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
