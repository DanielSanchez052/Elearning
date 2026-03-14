using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IPasswordHasherService> _hasherMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_usersMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static User BuildVerifiedUser(string email = "user@test.com", string hash = "hashed")
    {
        var user = User.Create("Test User", email, hash, countryId: 1);
        user.SetEmailVerifyToken("token");
        user.VerifyEmail();
        return user;
    }

    private void SetupJwt(User user)
    {
        _jwtMock
            .Setup(j => j.GenerateAccessToken(user))
            .Returns(("jwt-token-value", DateTime.UtcNow.AddHours(1)));
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsSuccess()
    {
        var user = BuildVerifiedUser();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);
        SetupJwt(user);

        var result = await _handler.HandleAsync(new LoginCommand("user@test.com", "password123"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("jwt-token-value", result.Value!.AccessToken);
        Assert.Equal("user@test.com", result.Value.User.Email);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsUnauthorized()
    {
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("notfound@test.com", default))
                  .ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(new LoginCommand("notfound@test.com", "pass"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Unauthorized, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsUnauthorized()
    {
        var user = BuildVerifiedUser();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "wrongpass")).Returns(false);

        var result = await _handler.HandleAsync(new LoginCommand("user@test.com", "wrongpass"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Unauthorized, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_SameErrorMessageAsUserNotFound()
    {
        // Anti-enumeración: no revelar si el email existe o no
        var user = BuildVerifiedUser();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "wrongpass")).Returns(false);
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("ghost@test.com", default)).ReturnsAsync((User?)null);

        var resultWrongPass = await _handler.HandleAsync(new LoginCommand("user@test.com", "wrongpass"));
        var resultNotFound = await _handler.HandleAsync(new LoginCommand("ghost@test.com", "anypass"));

        Assert.Equal(resultWrongPass.Error, resultNotFound.Error);
    }

    [Fact]
    public async Task HandleAsync_EmailNotVerified_ReturnsUnauthorized()
    {
        var user = User.Create("Test User", "user@test.com", "hashed", countryId: 1);
        // No llamamos VerifyEmail() → IsEmailVerified = false
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);

        var result = await _handler.HandleAsync(new LoginCommand("user@test.com", "password123"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Unauthorized, result.ErrorType);
        Assert.Contains("verificar", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_SuccessfulLogin_CallsUpdateAsync()
    {
        var user = BuildVerifiedUser();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);
        SetupJwt(user);

        await _handler.HandleAsync(new LoginCommand("user@test.com", "password123"));

        _usersMock.Verify(r => r.UpdateAsync(user, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SuccessfulLogin_IncrementsLoginStreak()
    {
        var user = BuildVerifiedUser();
        var streakBefore = user.LoginStreak;
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);
        SetupJwt(user);

        await _handler.HandleAsync(new LoginCommand("user@test.com", "password123"));

        Assert.Equal(streakBefore + 1, user.LoginStreak);
        Assert.NotNull(user.LastLoginAt);
    }
}