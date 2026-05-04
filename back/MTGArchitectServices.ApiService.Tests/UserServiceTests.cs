using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MTGArchitect.Data.Models;
using MTGArchitect.Data.Repositories;
using NSubstitute;

namespace MTGArchitectServices.ApiService.Tests;

[TestFixture]
public class UserServiceTests
{
    private IUserRepository _repository = null!;
    private UserService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IUserRepository>();
        _service = new UserService(_repository);
    }

    [Test]
    public async Task GetSettingsAsync_WhenUserExists_ReturnsOk()
    {
        var user = CreateUser();
        _repository.GetByIdWithSettingsAndDecksAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _service.GetSettingsAsync(CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
        Assert.That((result as IValueHttpResult)?.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetSettingsAsync_WhenUserNotFound_ReturnsNotFound()
    {
        _repository.GetByIdWithSettingsAndDecksAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ApplicationUser?)null);

        var result = await _service.GetSettingsAsync(CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetSettingsAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var result = await _service.GetSettingsAsync(CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task GetDecksAsync_WhenUserExists_ReturnsOk()
    {
        var user = CreateUser();
        _repository.GetByIdWithSettingsAndDecksAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _service.GetDecksAsync(CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetDecksAsync_WhenUserNotFound_ReturnsNotFound()
    {
        _repository.GetByIdWithSettingsAndDecksAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ApplicationUser?)null);

        var result = await _service.GetDecksAsync(CreatePrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetDecksAsync_WhenUnauthorized_ReturnsUnauthorized()
    {
        var result = await _service.GetDecksAsync(CreateAnonymousPrincipal(), default);

        Assert.That((result as IStatusCodeHttpResult)?.StatusCode, Is.EqualTo(401));
    }

    private static ClaimsPrincipal CreatePrincipal(string userId = "user-1") =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)]));

    private static ClaimsPrincipal CreateAnonymousPrincipal() =>
        new(new ClaimsIdentity());

    private static ApplicationUser CreateUser() => new()
    {
        Id = "user-1",
        DisplayName = "Test User",
        Language = "en",
        Theme = "dark",
        DeckWorkspace = []
    };
}
