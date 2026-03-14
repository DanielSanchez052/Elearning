using ELearning.Application.Features.Countries.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Countries;

public class GetCountriesHandlerTests
{
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly GetCountriesHandler _handler;

    public GetCountriesHandlerTests() =>
        _handler = new GetCountriesHandler(_countriesMock.Object);

    private void SetupCountries(params Country[] countries)
    {
        _countriesMock
            .Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(countries);
    }

    [Fact]
    public async Task HandleAsync_OnlyActive_False_ReturnsAllCountries()
    {
        var active = CountryHelpers.BuildActive(1, "COL", "Colombia");
        var inactive = CountryHelpers.BuildInactive(2, "ARG", "Argentina");
        SetupCountries(active, inactive);

        var result = await _handler.HandleAsync(new GetCountriesQuery(OnlyActive: false));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task HandleAsync_OnlyActive_True_ReturnsOnlyActiveCountries()
    {
        var active = CountryHelpers.BuildActive(1, "COL", "Colombia");
        var inactive = CountryHelpers.BuildInactive(2, "ARG", "Argentina");
        SetupCountries(active, inactive);

        var result = await _handler.HandleAsync(new GetCountriesQuery(OnlyActive: true));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Colombia", result.Value[0].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyRepository_ReturnsEmptyList()
    {
        SetupCountries();

        var result = await _handler.HandleAsync(new GetCountriesQuery());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_ResultsOrderedByNameAscending()
    {
        var z = CountryHelpers.BuildActive(1, "ZZZ", "Zimbabwe");
        var a = CountryHelpers.BuildActive(2, "AAA", "Albania");
        var m = CountryHelpers.BuildActive(3, "MEX", "México");
        SetupCountries(z, a, m);

        var result = await _handler.HandleAsync(new GetCountriesQuery());

        var names = result.Value.Select(c => c.Name).ToList();
        Assert.Equal(new[] { "Albania", "México", "Zimbabwe" }, names);
    }

    [Fact]
    public async Task HandleAsync_CountriesMappedToDto_CorrectFields()
    {
        var country = CountryHelpers.BuildActive(1, "COL", "Colombia");
        SetupCountries(country);

        var result = await _handler.HandleAsync(new GetCountriesQuery());

        var dto = result.Value[0];
        Assert.Equal(1, dto.Id);
        Assert.Equal("COL", dto.Code);
        Assert.Equal("Colombia", dto.Name);
        Assert.True(dto.IsActive);
    }

    [Fact]
    public async Task HandleAsync_DefaultQuery_ReturnsBothActiveAndInactive()
    {
        // OnlyActive default = false
        var active = CountryHelpers.BuildActive(1);
        var inactive = CountryHelpers.BuildInactive(2);
        SetupCountries(active, inactive);

        var result = await _handler.HandleAsync(new GetCountriesQuery());

        Assert.Equal(2, result.Value.Count);
    }
}
