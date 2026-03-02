using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Admin.Commands;

public sealed record ChangeUserCountryCommand(
    Guid TargetUserId,
    int NewCountryId
) : ICommand;

public sealed class ChangeUserCountryHandler : ICommandHandler<ChangeUserCountryCommand>
{
    private readonly IUserRepository _users;
    private readonly ICountryRepository _countries;

    public ChangeUserCountryHandler(IUserRepository users, ICountryRepository countries)
    {
        _users = users;
        _countries = countries;
    }

    public async Task<Result> HandleAsync(ChangeUserCountryCommand cmd, CancellationToken ct = default)
    {
        var country = await _countries.GetByIdAsync(cmd.NewCountryId, ct);
        if (country is null)
            return Result.NotFound($"País con id '{cmd.NewCountryId}' no encontrado.");

        if (!country.IsActive)
            return Result.Conflict($"El país '{country.Name}' está desactivado y no puede asignarse.");

        var target = await _users.GetByIdTrackedAsync(cmd.TargetUserId, ct);
        if (target is null)
            return Result.NotFound($"Usuario con id '{cmd.TargetUserId}' no encontrado.");

        if (target.CountryId == cmd.NewCountryId)
            return Result.Conflict($"El usuario ya pertenece al país '{country.Name}'.");

        target.SetCountry(cmd.NewCountryId);
        await _users.UpdateAsync(target, ct);

        return Result.Success();
    }
}
