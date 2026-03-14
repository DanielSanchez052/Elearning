using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.Queries.GetCurrentUser;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class GetCurrentUserHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly GetCurrentUserHandler _handler;

    public GetCurrentUserHandlerTests() =>
        _handler = new GetCurrentUserHandler(_usersMock.Object);

    [Fact]
    public async Task HandleAsync_ExistingUser_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var country = Country.Create("CO", "Colombia");
        Helpers.SetPrivate(country, "Id", 1);
        var user = User.Create("Ana García", "ana@test.com", "hashed", countryId: 1);
        // Forzamos el Id (el User.Create usa Guid.NewGuid() internamente)
        // Si Id tiene setter privado, usamos reflexión para el test
        SetUserId(user, userId);
        SetCountry(user, country);

        _usersMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);

        var result = await _handler.HandleAsync(new GetCurrentUserQuery(userId));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Ana García", result.Value!.FullName);
        Assert.Equal("ana@test.com", result.Value.Email);
        Assert.Equal("Colombia", result.Value.Country);
        Assert.Equal("student", result.Value.Role); // ToLowerInvariant()
        Assert.Equal(1, result.Value.CountryId);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        _usersMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(new GetCurrentUserQuery(userId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains(userId.ToString(), result.Error);
    }

    // ── Helpers de reflexión ─────────────────────────────────────────────

    private static void SetUserId(User user, Guid id)
    {
        var prop = typeof(User).GetProperty(nameof(User.Id));
        prop?.SetValue(user, id);
    }

    private static void SetCountry(User user, Country country)
    {
        var prop = typeof(User).GetProperty(nameof(User.Country));
        prop?.SetValue(user, country);
    }
}



