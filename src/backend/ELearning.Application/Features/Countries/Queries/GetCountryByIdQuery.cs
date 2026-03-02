using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Dtos;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Countries.Queries;

public sealed record GetCountryByIdQuery(int CountryId) : IQuery<CountryDto>;

public sealed class GetCountryByIdHandler : IQueryHandler<GetCountryByIdQuery, CountryDto>
{
    private readonly ICountryRepository _countries;

    public GetCountryByIdHandler(ICountryRepository countries)
    {
        _countries = countries;
    }

    public async Task<Result<CountryDto>> HandleAsync(
        GetCountryByIdQuery query,
        CancellationToken ct = default)
    {
        var country = await _countries.GetByIdAsync(query.CountryId, ct);
        
        if (country is null)
            return Result.NotFound<CountryDto>($"País con id '{query.CountryId}' no encontrado.");

        return Result.Success(new CountryDto(
            country.Id,
            country.Code,
            country.Name,
            country.IsActive
        ));
    }
}
