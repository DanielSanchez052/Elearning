using ELearning.Application.Features.Auth.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _handler = new ForgotPasswordHandler(_usersMock.Object, _emailMock.Object);
        _emailMock
            .Setup(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task HandleAsync_ExistingEmail_ReturnsSuccess()
    {
        var user = User.Create("Test User", "user@test.com", "hashed", countryId: 1);
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);

        var result = await _handler.HandleAsync(new ForgotPasswordCommand("user@test.com"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ExistingEmail_SetsResetTokenWithExpiry()
    {
        var user = User.Create("Test User", "user@test.com", "hashed", countryId: 1);
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);

        var before = DateTime.UtcNow;
        await _handler.HandleAsync(new ForgotPasswordCommand("user@test.com"));

        Assert.NotNull(user.ResetToken);
        Assert.NotNull(user.ResetTokenExpires);
        // Expira ~15 minutos en el futuro (tolerancia de 5 segs)
        Assert.True(user.ResetTokenExpires > before.AddMinutes(14));
        Assert.True(user.ResetTokenExpires < before.AddMinutes(16));
    }

    [Fact]
    public async Task HandleAsync_ExistingEmail_TokenIs32HexChars()
    {
        var user = User.Create("Test User", "user@test.com", "hashed", countryId: 1);
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);

        await _handler.HandleAsync(new ForgotPasswordCommand("user@test.com"));

        // Guid.NewGuid().ToString("N") produce exactamente 32 hex chars sin guiones
        Assert.NotNull(user.ResetToken);
        Assert.Equal(32, user.ResetToken!.Length);
        Assert.Matches("^[0-9a-f]{32}$", user.ResetToken);
    }

    [Fact]
    public async Task HandleAsync_NonExistingEmail_ReturnsSuccessWithoutSendingEmail()
    {
        // Anti-enumeración: no revelar si el email existe
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("ghost@test.com", default))
                  .ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(new ForgotPasswordCommand("ghost@test.com"));

        Assert.True(result.IsSuccess);
        _emailMock.Verify(
            e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ExistingEmail_SendsResetEmailWithCorrectToken()
    {
        var user = User.Create("Test User", "user@test.com", "hashed", countryId: 1);
        _usersMock.Setup(r => r.GetByEmailTrackedAsync("user@test.com", default)).ReturnsAsync(user);

        string? capturedToken = null;
        _emailMock
            .Setup(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, _, token, _) => capturedToken = token)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(new ForgotPasswordCommand("user@test.com"));

        Assert.Equal(user.ResetToken, capturedToken);
    }
}
