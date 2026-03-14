using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Users.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Users;

public class ChangeUserCountryHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly ChangeUserCountryHandler _handler;

    private static readonly Guid TargetId = Guid.NewGuid();

    public ChangeUserCountryHandlerTests() =>
        _handler = new ChangeUserCountryHandler(_usersMock.Object, _countriesMock.Object);

    private void SetupCountry(Country country) =>
        _countriesMock
            .Setup(r => r.GetByIdAsync(country.Id, default))
            .ReturnsAsync(country);

    private void SetupTarget(User target) =>
        _usersMock
            .Setup(r => r.GetByIdTrackedAsync(target.Id, default))
            .ReturnsAsync(target);

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidCountryAndUser_ReturnsSuccess()
    {
        var country = UserHelpers.BuildCountry(id: 2, name: "México", active: true);
        var target = UserHelpers.BuildUser(id: TargetId, countryId: 1);
        SetupCountry(country);
        SetupTarget(target);

        var result = await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 2));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidChange_CallsUpdateAsync()
    {
        var country = UserHelpers.BuildCountry(id: 2, active: true);
        var target = UserHelpers.BuildUser(id: TargetId, countryId: 1);
        SetupCountry(country);
        SetupTarget(target);

        await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 2));

        _usersMock.Verify(r => r.UpdateAsync(target, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidChange_CountryIdUpdatedOnEntity()
    {
        var country = UserHelpers.BuildCountry(id: 2, active: true);
        var target = UserHelpers.BuildUser(id: TargetId, countryId: 1);
        SetupCountry(country);
        SetupTarget(target);

        await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 2));

        Assert.Equal(2, target.CountryId);
    }

    // ── País no encontrado ────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_CountryNotFound_ReturnsNotFound()
    {
        _countriesMock
            .Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Country?)null);

        var result = await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 99));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains("99", result.Error);
    }

    // ── País inactivo ─────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_InactiveCountry_ReturnsConflict()
    {
        var inactive = UserHelpers.BuildCountry(id: 2, name: "Argentina", active: false);
        SetupCountry(inactive);

        var result = await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 2));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
        Assert.Contains("Argentina", result.Error);
    }

    // ── Usuario no encontrado ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        var country = UserHelpers.BuildCountry(id: 2, active: true);
        SetupCountry(country);
        _usersMock
            .Setup(r => r.GetByIdTrackedAsync(TargetId, default))
            .ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 2));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains(TargetId.ToString(), result.Error);
    }

    // ── Ya está en ese país ────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserAlreadyInCountry_ReturnsConflict()
    {
        var country = UserHelpers.BuildCountry(id: 1, name: "Colombia", active: true);
        var target = UserHelpers.BuildUser(id: TargetId, countryId: 1);
        SetupCountry(country);
        SetupTarget(target);

        var result = await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
        Assert.Contains("Colombia", result.Error);
    }

    // ── Orden de checks: país primero ──────────────────────────────────────

    [Fact]
    public async Task HandleAsync_CountryNotFound_NeverQueriesUser()
    {
        _countriesMock
            .Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Country?)null);

        await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 99));

        _usersMock.Verify(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── No llama UpdateAsync cuando falla ─────────────────────────────────

    [Fact]
    public async Task HandleAsync_WhenConflict_NeverCallsUpdateAsync()
    {
        var inactive = UserHelpers.BuildCountry(id: 2, active: false);
        SetupCountry(inactive);

        await _handler.HandleAsync(new ChangeUserCountryCommand(TargetId, 2));

        _usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
