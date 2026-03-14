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

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly Mock<IPasswordHasherService> _hasherMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserHandler(
            _usersMock.Object,
            _countriesMock.Object,
            _hasherMock.Object,
            _emailMock.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static RegisterUserCommand ValidCmd() =>
        new("John Doe", "john@test.com", "Password1", CountryId: 1);

    private void SetupValidScenario()
    {
        var country = Country.Create("CO", "Colombia");
        Helpers.SetPrivate(country, "Id", 1);
        _countriesMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(country);
        _usersMock.Setup(r => r.ExistsByEmailAsync("john@test.com", default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("Password1")).Returns("hashed-password");
        _usersMock.Setup(r => r.CreateAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);
        _emailMock
            .Setup(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidData_ReturnsSuccessWithGuid()
    {
        SetupValidScenario();

        var result = await _handler.HandleAsync(ValidCmd());

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task HandleAsync_CountryNotFound_ReturnsNotFound()
    {
        _countriesMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Country?)null);

        var result = await _handler.HandleAsync(ValidCmd() with { CountryId = 99 });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains("99", result.Error);
    }

    [Fact]
    public async Task HandleAsync_EmailAlreadyTaken_ReturnsConflict()
    {
        var country = Country.Create("CO", "Colombia");
        Helpers.SetPrivate(country, "Id", 1);
        _countriesMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(country);
        _usersMock.Setup(r => r.ExistsByEmailAsync("john@test.com", default)).ReturnsAsync(true);

        var result = await _handler.HandleAsync(ValidCmd());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType                );
        Assert.Contains("john@test.com", result.Error);
    }

    [Fact]
    public async Task HandleAsync_ValidData_EmailNormalizedToLowercase()
    {
        var country = Country.Create("CO", "Colombia");
        Helpers.SetPrivate(country, "Id", 1);
        _countriesMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(country);
        _usersMock.Setup(r => r.ExistsByEmailAsync("JOHN@TEST.COM", default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _usersMock.Setup(r => r.CreateAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);
        _emailMock.Setup(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        User? createdUser = null;
        _usersMock.Setup(r => r.CreateAsync(It.IsAny<User>(), default))
                  .Callback<User, CancellationToken>((u, _) => createdUser = u)
                  .Returns(Task.CompletedTask);

        var cmd = new RegisterUserCommand("John Doe", "JOHN@TEST.COM", "Password1", CountryId: 1);
        await _handler.HandleAsync(cmd);

        Assert.NotNull(createdUser);
        Assert.Equal("john@test.com", createdUser!.Email);
    }

    [Fact]
    public async Task HandleAsync_ValidData_PasswordIsHashed()
    {
        SetupValidScenario();

        User? createdUser = null;
        _usersMock.Setup(r => r.CreateAsync(It.IsAny<User>(), default))
                  .Callback<User, CancellationToken>((u, _) => createdUser = u)
                  .Returns(Task.CompletedTask);

        await _handler.HandleAsync(ValidCmd());

        Assert.NotNull(createdUser);
        Assert.Equal("hashed-password", createdUser!.PasswordHash);
        // La contraseña en texto plano nunca debe estar en el hash
        Assert.DoesNotContain("Password1", createdUser.PasswordHash);
    }

    [Fact]
    public async Task HandleAsync_ValidData_EmailVerifyTokenIsSet()
    {
        SetupValidScenario();

        User? createdUser = null;
        _usersMock.Setup(r => r.CreateAsync(It.IsAny<User>(), default))
                  .Callback<User, CancellationToken>((u, _) => createdUser = u)
                  .Returns(Task.CompletedTask);

        await _handler.HandleAsync(ValidCmd());

        Assert.NotNull(createdUser);
        Assert.NotNull(createdUser!.EmailVerifyToken);
        Assert.NotEmpty(createdUser.EmailVerifyToken!);
    }

    [Fact]
    public async Task HandleAsync_ValidData_SendsVerificationEmail()
    {
        SetupValidScenario();

        await _handler.HandleAsync(ValidCmd());

        _emailMock.Verify(
            e => e.SendEmailVerificationAsync(
                "john@test.com",
                It.IsAny<string>(),
                It.IsAny<string>(),
                CancellationToken.None),   // fire & forget usa CancellationToken.None
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidData_EmailServiceUsesCancellationTokenNone()
    {
        SetupValidScenario();

        // Capturamos el CancellationToken que recibe el email service
        CancellationToken capturedCt = default;
        _emailMock
            .Setup(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, _, _, ct) => capturedCt = ct)
            .Returns(Task.CompletedTask);

        // Usamos un token cancelado para simular que el request fue abortado
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await _handler.HandleAsync(ValidCmd(), cts.Token);

        // El email service NO debe recibir el token cancelado
        Assert.Equal(CancellationToken.None, capturedCt);
    }
}