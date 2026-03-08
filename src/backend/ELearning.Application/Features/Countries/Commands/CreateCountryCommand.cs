using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Countries.Commands;

public sealed record CreateCountryCommand(
    string Code,
    string Name
) : ICommand<int>;

public sealed class CreateCountryHandler : ICommandHandler<CreateCountryCommand, int>
{
    private readonly ICountryRepository _countries;

    public CreateCountryHandler(ICountryRepository countries)
    {
        _countries = countries;
    }

    public async Task<Result<int>> HandleAsync(CreateCountryCommand cmd, CancellationToken ct = default)
    {
        var codeExists = await _countries.ExistsByCodeAsync(cmd.Code.ToLower(), ct);
        if (codeExists)
            return Result.Conflict<int>($"Ya existe un país con el código '{cmd.Code.ToUpperInvariant()}'.");

        var country = Country.Create(
            code: cmd.Code.ToUpperInvariant(),
            name: cmd.Name.Trim()
        );

        await _countries.CreateAsync(country, ct);

        return Result.Success(country.Id);
    }
}
