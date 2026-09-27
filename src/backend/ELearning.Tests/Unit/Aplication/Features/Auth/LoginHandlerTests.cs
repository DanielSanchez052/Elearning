using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.Commands;
using ELearning.Application.Features.Gamification.DTOs;
using ELearning.Application.Features.Gamification.Services;
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
    private readonly Mock<IBadgeAwardService> _badgesMock = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_usersMock.Object, _hasherMock.Object, _jwtMock.Object, _badgesMock.Object);
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
        VerifyLoginBadgeHookNeverCalled();
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
        VerifyLoginBadgeHookNeverCalled();
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
        VerifyLoginBadgeHookNeverCalled();
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

    // ── Badge awarding (best-effort, after save) ─────────────────────────────
    // The handler links the request's ct to a 2s timeout before calling the
    // badge service, so mocks/verifies here match It.IsAny<CancellationToken>()
    // instead of `default` — the token instance passed through is never the
    // original one.

    [Fact]
    public async Task HandleAsync_SuccessfulLogin_CallsLoginBadgeHookOnceAfterSaving()
    {
        var user = BuildVerifiedUser();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);
        SetupJwt(user);
        var calls = new List<string>();
        _usersMock.Setup(r => r.UpdateAsync(user, default)).Callback(() => calls.Add("save")).Returns(Task.CompletedTask);
        _badgesMock
            .Setup(b => b.OnUserLoggedInAsync(user, It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("badge"))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new LoginCommand("user@test.com", "password123"));

        Assert.True(result.IsSuccess);
        _badgesMock.Verify(b => b.OnUserLoggedInAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(["save", "badge"], calls);
    }

    [Fact]
    public async Task HandleAsync_SuccessfulLogin_BadgeServiceThrows_LoginStillSucceeds()
    {
        var user = BuildVerifiedUser();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);
        SetupJwt(user);
        _badgesMock
            .Setup(b => b.OnUserLoggedInAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("badge store down"));

        var result = await _handler.HandleAsync(new LoginCommand("user@test.com", "password123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token-value", result.Value!.AccessToken);
    }

    [Fact]
    public async Task HandleAsync_BadgeServiceTimesOut_LoginStillSucceeds()
    {
        var user = BuildVerifiedUser();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);
        SetupJwt(user);
        _badgesMock
            .Setup(b => b.OnUserLoggedInAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns<User, CancellationToken>(async (_, ct) =>
            {
                // Simulates a badge store slower than the 2s bound: the linked
                // token trips before this completes, never a real client cancel.
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                return (IReadOnlyList<AwardedBadgeDto>)Array.Empty<AwardedBadgeDto>();
            });

        var result = await _handler.HandleAsync(new LoginCommand("user@test.com", "password123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token-value", result.Value!.AccessToken);
    }

    [Fact]
    public async Task HandleAsync_RealClientCancellation_PropagatesInsteadOfSwallowing()
    {
        var user = BuildVerifiedUser();
        using var cts = new CancellationTokenSource();
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", cts.Token)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(user.PasswordHash, "password123")).Returns(true);
        SetupJwt(user);
        _badgesMock
            .Setup(b => b.OnUserLoggedInAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns<User, CancellationToken>((_, _) =>
            {
                cts.Cancel(); // the real request ct itself gets cancelled mid-call
                throw new OperationCanceledException(cts.Token);
            });

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.HandleAsync(new LoginCommand("user@test.com", "password123"), cts.Token));
    }

    private void VerifyLoginBadgeHookNeverCalled() =>
        _badgesMock.Verify(b => b.OnUserLoggedInAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
}