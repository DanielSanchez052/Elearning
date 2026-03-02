using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Dtos;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Countries.Queries;

// Este Query tal vez se pueda dividir en dos: uno para admin (que muestra todos) y otro para público (que muestra solo activos). Por ahora lo dejo con un flag, pero si se complica se puede dividir.

public sealed record GetCountriesQuery(
    bool OnlyActive = false  // false = todos (admin), true = solo activos (público)
) : IQuery<IReadOnlyList<CountryDto>>;


public sealed class GetCountriesHandler
    : IQueryHandler<GetCountriesQuery, IReadOnlyList<CountryDto>>
{
    private readonly ICountryRepository _countries;

    public GetCountriesHandler(ICountryRepository countries)
    {
        _countries = countries;
    }

    public async Task<Result<IReadOnlyList<CountryDto>>> HandleAsync(
        GetCountriesQuery query,
        CancellationToken ct = default)
    {
        var countries = await _countries.GetAllAsync(ct);

        var filtered = query.OnlyActive
            ? countries.Where(c => c.IsActive)
            : countries;

        var dtos = filtered
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto(c.Id, c.Code, c.Name, c.IsActive))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<CountryDto>>(dtos);
    }
}