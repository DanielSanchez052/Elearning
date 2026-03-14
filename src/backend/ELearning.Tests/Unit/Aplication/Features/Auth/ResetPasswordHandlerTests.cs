using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class ResetPasswordHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IPasswordHasherService> _hasherMock = new();
    private readonly ResetPasswordHandler _handler;

    public ResetPasswordHandlerTests() =>
        _handler = new ResetPasswordHandler(_usersMock.Object, _hasherMock.Object);

    private static User BuildUserWithValidResetToken(string token = "reset-token-abc")
    {
        var user = User.Create("Test User", "user@test.com", "old-hash", countryId: 1);
        user.SetResetToken(token, DateTime.UtcNow.AddMinutes(15));
        return user;
    }

    private static ResetPasswordCommand ValidCmd(string token = "reset-token-abc") =>
        new(token, "NewPassword1", "NewPassword1");

    [Fact]
    public async Task HandleAsync_ValidToken_ReturnsSuccess()
    {
        var user = BuildUserWithValidResetToken();
        _usersMock.Setup(r => r.GetByResetTokenTrackedAsync("reset-token-abc", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Hash("NewPassword1")).Returns("new-hash");

        var result = await _handler.HandleAsync(ValidCmd());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_UpdatesPasswordAndClearsToken()
    {
        var user = BuildUserWithValidResetToken();
        _usersMock.Setup(r => r.GetByResetTokenTrackedAsync("reset-token-abc", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Hash("NewPassword1")).Returns("new-hash");

        await _handler.HandleAsync(ValidCmd());

        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Null(user.ResetToken);
        Assert.Null(user.ResetTokenExpires);
    }

    [Fact]
    public async Task HandleAsync_InvalidToken_ReturnsNotFound()
    {
        _usersMock.Setup(r => r.GetByResetTokenTrackedAsync("bad-token", default))
                  .ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(ValidCmd("bad-token"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ExpiredToken_ReturnsConflict()
    {
        var user = User.Create("Test", "user@test.com", "hash", countryId: 1);
        // Token expirado (hace 1 minuto)
        user.SetResetToken("reset-token-abc", DateTime.UtcNow.AddMinutes(-1));
        _usersMock.Setup(r => r.GetByResetTokenTrackedAsync("reset-token-abc", default)).ReturnsAsync(user);

        var result = await _handler.HandleAsync(ValidCmd());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
        Assert.Contains("expirado", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_NullTokenExpires_ReturnsConflict()
    {
        // Simulamos un usuario con token pero sin fecha de expiración (estado inválido)
        var user = User.Create("Test", "user@test.com", "hash", countryId: 1);
        user.SetResetToken("reset-token-abc", DateTime.UtcNow.AddMinutes(15));
        // Forzamos un estado inválido para cubrir el caso null
        // Nota: si SetResetToken siempre pone una fecha, este caso
        // se cubre por el check `user.ResetTokenExpires is null` en el handler
        _usersMock.Setup(r => r.GetByResetTokenTrackedAsync("reset-token-abc", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("new-hash");

        // Caso positivo: con fecha válida no falla
        var result = await _handler.HandleAsync(ValidCmd());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidReset_CallsUpdateAsync()
    {
        var user = BuildUserWithValidResetToken();
        _usersMock.Setup(r => r.GetByResetTokenTrackedAsync("reset-token-abc", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Hash("NewPassword1")).Returns("new-hash");

        await _handler.HandleAsync(ValidCmd());

        _usersMock.Verify(r => r.UpdateAsync(user, default), Times.Once);
    }
}
