using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MTGArchitect.Scryfall.Service.Tests;

public class CardSearchTests
{
    private IHttpClientFactory _httpClientFactory = null!;
    private ILogger<CardController> _logger = null!;
    private CardController _controller = null!;
    private HttpClient _httpClient = null!;

    [SetUp]
    public void Setup()
    {
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _logger = Substitute.For<ILogger<CardController>>();
        _controller = new CardController(_httpClientFactory, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [Test]
    public async Task SearchCards_WithValidQuery_ReturnsScryfallCards()
    {
        var scryfallResponse = """
            {
                "object": "list",
                "total_cards": 2,
                "has_more": false,
                "data": [
                    {
                        "id": "ea8d7a04-1939-401a-b9a6-cd8c5a5d9f57",
                        "name": "Black Lotus",
                        "mana_cost": "{0}",
                        "type_line": "Artifact",
                        "set": "lea",
                        "image_uris": {
                            "small": "https://cards.scryfall.io/small/front/b/d/bd8fa327-dd41-4737-8f19-2cf5eb1f7cdd.jpg",
                            "normal": "https://cards.scryfall.io/normal/front/b/d/bd8fa327-dd41-4737-8f19-2cf5eb1f7cdd.jpg",
                            "large": "https://cards.scryfall.io/large/front/b/d/bd8fa327-dd41-4737-8f19-2cf5eb1f7cdd.jpg"
                        }
                    },
                    {
                        "id": "f06d97f3-3cd9-4e8d-8a7b-0c3a6f111b66",
                        "name": "Lotus Petal",
                        "mana_cost": "{0}",
                        "type_line": "Artifact",
                        "set": "tmp",
                        "image_uris": {
                            "small": "https://cards.scryfall.io/small/front/f/8/f85ab5f9-508e-45de-8fa1-ce1f16552ffc.jpg",
                            "normal": "https://cards.scryfall.io/normal/front/f/8/f85ab5f9-508e-45de-8fa1-ce1f16552ffc.jpg",
                            "large": "https://cards.scryfall.io/large/front/f/8/f85ab5f9-508e-45de-8fa1-ce1f16552ffc.jpg"
                        }
                    }
                ]
            }
            """;

        var messageHandler = new MockHttpMessageHandler(scryfallResponse, HttpStatusCode.OK);
        _httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("https://api.scryfall.com/")
        };
        _httpClientFactory.CreateClient("scryfall").Returns(_httpClient);

        var result = await _controller.SearchCards("lotus", 20);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Has.Count.EqualTo(2));
            Assert.That(result.Cards[0].Name, Is.EqualTo("Black Lotus"));
            Assert.That(result.Cards[0].Id, Is.EqualTo("ea8d7a04-1939-401a-b9a6-cd8c5a5d9f57"));
            Assert.That(result.Cards[0].ManaCost, Is.EqualTo("{0}"));
            Assert.That(result.Cards[0].TypeLine, Is.EqualTo("Artifact"));
            Assert.That(result.Cards[0].SetCode, Is.EqualTo("lea"));
            Assert.That(result.Cards[0].ImageUrl, Does.Contain("scryfall.io"));
            Assert.That(result.Cards[1].Name, Is.EqualTo("Lotus Petal"));
        });
    }

    [Test]
    public async Task SearchCards_WithEmptyQuery_UsesDefaultQuery()
    {
        var scryfallResponse = """
            {
                "object": "list",
                "total_cards": 1,
                "has_more": false,
                "data": [
                    {
                        "id": "test-id",
                        "name": "Test Creature",
                        "mana_cost": "{1}{G}",
                        "type_line": "Creature — Test",
                        "set": "tst",
                        "image_uris": {
                            "normal": "https://cards.scryfall.io/normal/test.jpg"
                        }
                    }
                ]
            }
            """;

        var messageHandler = new MockHttpMessageHandler(scryfallResponse, HttpStatusCode.OK);
        _httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("https://api.scryfall.com/")
        };
        _httpClientFactory.CreateClient("scryfall").Returns(_httpClient);

        var result = await _controller.SearchCards("", 20);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Has.Count.EqualTo(1));
            Assert.That(messageHandler.RequestUri?.ToString(), Does.Contain("type%3Acreature"));
        });
    }

    [Test]
    public async Task SearchCards_WhenApiReturnsError_ReturnsEmptyResult()
    {
        var messageHandler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        _httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("https://api.scryfall.com/")
        };
        _httpClientFactory.CreateClient("scryfall").Returns(_httpClient);

        var result = await _controller.SearchCards("lotus", 20);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Is.Empty);
        });
    }

    [Test]
    public async Task SearchCards_WithPageSize_LimitsResults()
    {
        var scryfallResponse = """
            {
                "object": "list",
                "total_cards": 10,
                "has_more": false,
                "data": [
                    {"id": "1", "name": "Card 1", "mana_cost": "{0}", "type_line": "Artifact", "set": "tst"},
                    {"id": "2", "name": "Card 2", "mana_cost": "{0}", "type_line": "Artifact", "set": "tst"},
                    {"id": "3", "name": "Card 3", "mana_cost": "{0}", "type_line": "Artifact", "set": "tst"},
                    {"id": "4", "name": "Card 4", "mana_cost": "{0}", "type_line": "Artifact", "set": "tst"},
                    {"id": "5", "name": "Card 5", "mana_cost": "{0}", "type_line": "Artifact", "set": "tst"}
                ]
            }
            """;

        var messageHandler = new MockHttpMessageHandler(scryfallResponse, HttpStatusCode.OK);
        _httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("https://api.scryfall.com/")
        };
        _httpClientFactory.CreateClient("scryfall").Returns(_httpClient);

        var result = await _controller.SearchCards("artifact", 3);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public async Task SearchCards_WithCardWithoutImageUris_HandlesGracefully()
    {
        var scryfallResponse = """
            {
                "object": "list",
                "total_cards": 1,
                "has_more": false,
                "data": [
                    {
                        "id": "test-id",
                        "name": "Card Without Image",
                        "mana_cost": "{1}",
                        "type_line": "Artifact",
                        "set": "tst"
                    }
                ]
            }
            """;

        var messageHandler = new MockHttpMessageHandler(scryfallResponse, HttpStatusCode.OK);
        _httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("https://api.scryfall.com/")
        };
        _httpClientFactory.CreateClient("scryfall").Returns(_httpClient);

        var result = await _controller.SearchCards("test", 20);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Cards, Has.Count.EqualTo(1));
            Assert.That(result.Cards[0].ImageUrl, Is.Empty);
        });
    }

    private class MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
