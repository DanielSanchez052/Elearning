using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class VerifyEmailHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly VerifyEmailHandler _handler;

    public VerifyEmailHandlerTests() =>
        _handler = new VerifyEmailHandler(_usersMock.Object);

    private static User BuildUnverifiedUser()
    {
        var user = User.Create("Test User", "user@test.com", "hashed", countryId: 1);
        user.SetEmailVerifyToken("valid-token-abc123");
        return user;
    }

    [Fact]
    public async Task HandleAsync_ValidToken_ReturnsSuccess()
    {
        var user = BuildUnverifiedUser();
        _usersMock.Setup(r => r.GetByEmailVerifyTokenTrackedAsync("valid-token-abc123", default))
                  .ReturnsAsync(user);

        var result = await _handler.HandleAsync(new VerifyEmailCommand("valid-token-abc123"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_SetsEmailVerifiedAndClearsToken()
    {
        var user = BuildUnverifiedUser();
        _usersMock.Setup(r => r.GetByEmailVerifyTokenTrackedAsync("valid-token-abc123", default))
                  .ReturnsAsync(user);

        await _handler.HandleAsync(new VerifyEmailCommand("valid-token-abc123"));

        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerifyToken);
    }

    [Fact]
    public async Task HandleAsync_InvalidToken_ReturnsNotFound()
    {
        _usersMock.Setup(r => r.GetByEmailVerifyTokenTrackedAsync("bad-token", default))
                  .ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(new VerifyEmailCommand("bad-token"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_AlreadyVerified_ReturnsConflict()
    {
        var user = BuildUnverifiedUser();
        user.VerifyEmail(); // ya verificado
        _usersMock.Setup(r => r.GetByEmailVerifyTokenTrackedAsync("valid-token-abc123", default))
                  .ReturnsAsync(user);

        var result = await _handler.HandleAsync(new VerifyEmailCommand("valid-token-abc123"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_CallsUpdateAsync()
    {
        var user = BuildUnverifiedUser();
        _usersMock.Setup(r => r.GetByEmailVerifyTokenTrackedAsync("valid-token-abc123", default))
                  .ReturnsAsync(user);

        await _handler.HandleAsync(new VerifyEmailCommand("valid-token-abc123"));

        _usersMock.Verify(r => r.UpdateAsync(user, default), Times.Once);
    }
}
