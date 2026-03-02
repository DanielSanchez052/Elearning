using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Admin.Commands;

public sealed record ToggleCountryStatusCommand(
    int CountryId
) : ICommand;

public sealed class ToggleCountryStatusHandler : ICommandHandler<ToggleCountryStatusCommand>
{
    private readonly ICountryRepository _countries;

    public ToggleCountryStatusHandler(ICountryRepository countries)
    {
        _countries = countries;
    }

    public async Task<Result> HandleAsync(ToggleCountryStatusCommand cmd, CancellationToken ct = default)
    {
        var country = await _countries.GetByIdTrackedAsync(cmd.CountryId, ct);
        if (country is null)
            return Result.NotFound($"País con id '{cmd.CountryId}' no encontrado.");

        if (country.IsActive)
            country.Deactivate();
        else
            country.Activate();

        await _countries.UpdateAsync(country, ct);

        return Result.Success();
    }
}
