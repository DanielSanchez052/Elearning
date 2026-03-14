using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Countries;

public class GetCountryByIdHandlerTests
{
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly GetCountryByIdHandler _handler;

    public GetCountryByIdHandlerTests() =>
        _handler = new GetCountryByIdHandler(_countriesMock.Object);

    [Fact]
    public async Task HandleAsync_ExistingCountry_ReturnsDtoWithCorrectFields()
    {
        var country = CountryHelpers.BuildActive(1, "COL", "Colombia");
        _countriesMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(country);

        var result = await _handler.HandleAsync(new GetCountryByIdQuery(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("COL", result.Value.Code);
        Assert.Equal("Colombia", result.Value.Name);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public async Task HandleAsync_InactiveCountry_ReturnsDtoWithIsActiveFalse()
    {
        var country = CountryHelpers.BuildInactive(2, "ARG", "Argentina");
        _countriesMock.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(country);

        var result = await _handler.HandleAsync(new GetCountryByIdQuery(2));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
    }

    [Fact]
    public async Task HandleAsync_NotFound_ReturnsNotFound()
    {
        _countriesMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Country?)null);

        var result = await _handler.HandleAsync(new GetCountryByIdQuery(99));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains("99", result.Error);
    }
}
