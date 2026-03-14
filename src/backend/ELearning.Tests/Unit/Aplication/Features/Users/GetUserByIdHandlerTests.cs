using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Users.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Users;

public class GetUserByIdHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly GetUserByIdHandler _handler;

    public GetUserByIdHandlerTests() =>
        _handler = new GetUserByIdHandler(_usersMock.Object);

    [Fact]
    public async Task HandleAsync_ExistingUser_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var user = UserHelpers.BuildUser(id: userId);
        _usersMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);

        var result = await _handler.HandleAsync(new GetUserByIdQuery(userId));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ExistingUser_DtoMappedCorrectly()
    {
        var userId = Guid.NewGuid();
        var user = UserHelpers.BuildUser(
            id: userId,
            fullName: "Carlos Ruiz",
            email: "carlos@example.com",
            role: UserRole.Admin,
            countryId: 2,
            countryName: "México",
            verified: false);
        _usersMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);

        var result = await _handler.HandleAsync(new GetUserByIdQuery(userId));

        var dto = result.Value;
        Assert.Equal(userId, dto.Id);
        Assert.Equal("Carlos Ruiz", dto.FullName);
        Assert.Equal("carlos@example.com", dto.Email);
        Assert.Equal("admin", dto.Role);
        Assert.Equal("México", dto.Country);
        Assert.Equal(2, dto.CountryId);
        Assert.False(dto.IsEmailVerified);
    }

    [Fact]
    public async Task HandleAsync_NotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        _usersMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(new GetUserByIdQuery(userId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains(userId.ToString(), result.Error);
    }

    [Fact]
    public async Task HandleAsync_RoleInDto_IsAlwaysLowercase()
    {
        var userId = Guid.NewGuid();
        var user = UserHelpers.BuildUser(id: userId, role: UserRole.SuperAdmin);
        _usersMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);

        var result = await _handler.HandleAsync(new GetUserByIdQuery(userId));

        Assert.Equal("superadmin", result.Value.Role);
    }
}
