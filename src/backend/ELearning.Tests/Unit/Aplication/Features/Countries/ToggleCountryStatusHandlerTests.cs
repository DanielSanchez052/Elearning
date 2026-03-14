using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Countries;

public class ToggleCountryStatusHandlerTests
{
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly ToggleCountryStatusHandler _handler;

    public ToggleCountryStatusHandlerTests() =>
        _handler = new ToggleCountryStatusHandler(_countriesMock.Object);

    [Fact]
    public async Task HandleAsync_ActiveCountry_DeactivatesAndReturnsSuccess()
    {
        var country = CountryHelpers.BuildActive(id: 1);
        _countriesMock
            .Setup(r => r.GetByIdTrackedAsync(1, default))
            .ReturnsAsync(country);

        var result = await _handler.HandleAsync(new ToggleCountryStatusCommand(1));

        Assert.True(result.IsSuccess);
        Assert.False(country.IsActive);
    }

    [Fact]
    public async Task HandleAsync_InactiveCountry_ActivatesAndReturnsSuccess()
    {
        var country = CountryHelpers.BuildInactive(id: 2);
        _countriesMock
            .Setup(r => r.GetByIdTrackedAsync(2, default))
            .ReturnsAsync(country);

        var result = await _handler.HandleAsync(new ToggleCountryStatusCommand(2));

        Assert.True(result.IsSuccess);
        Assert.True(country.IsActive);
    }

    [Fact]
    public async Task HandleAsync_CountryNotFound_ReturnsNotFound()
    {
        _countriesMock
            .Setup(r => r.GetByIdTrackedAsync(99, default))
            .ReturnsAsync((Country?)null);

        var result = await _handler.HandleAsync(new ToggleCountryStatusCommand(99));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains("99", result.Error);
    }

    [Fact]
    public async Task HandleAsync_Toggle_CallsUpdateAsync()
    {
        var country = CountryHelpers.BuildActive(id: 1);
        _countriesMock
            .Setup(r => r.GetByIdTrackedAsync(1, default))
            .ReturnsAsync(country);

        await _handler.HandleAsync(new ToggleCountryStatusCommand(1));

        _countriesMock.Verify(r => r.UpdateAsync(country, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ToggledTwice_ReturnsToOriginalState()
    {
        var country = CountryHelpers.BuildActive(id: 1);
        _countriesMock
            .Setup(r => r.GetByIdTrackedAsync(1, default))
            .ReturnsAsync(country);

        await _handler.HandleAsync(new ToggleCountryStatusCommand(1));
        await _handler.HandleAsync(new ToggleCountryStatusCommand(1));

        Assert.True(country.IsActive); // vuelve a activo
    }
}
