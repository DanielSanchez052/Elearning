using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Domain.Constants;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Users.Commands;

public sealed record ChangeUserRoleCommand(
    Guid TargetUserId,
    string NewRole,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : ICommand;

/// <summary>Reglas de quién cambia el rol de quién: ver <see cref="UserAccess"/>.</summary>
public sealed class ChangeUserRoleHandler : ICommandHandler<ChangeUserRoleCommand>
{
    private readonly IUserRepository _users;

    public ChangeUserRoleHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result> HandleAsync(ChangeUserRoleCommand cmd, CancellationToken ct = default)
    {
        // El validador ya garantiza un NewRole válido.
        if (!RoleNames.TryParse(cmd.NewRole, out var newRole))
            return Result.ValidationFailure("Rol inválido.");

        if (!UserAccess.CanAssignRole(cmd.RequesterRole, newRole))
            return Result.Forbidden(UserAccess.AssignRoleForbiddenMessage);

        var target = await _users.GetByIdTrackedAsync(cmd.TargetUserId, ct);
        if (target is null || !UserAccess.CanView(target, cmd.RequesterRole, cmd.RequesterCountryId))
            return Result.NotFound($"Usuario con id '{cmd.TargetUserId}' no encontrado.");

        if (target.Id == cmd.RequesterId)
            return Result.Forbidden(UserAccess.OwnRoleForbiddenMessage);

        if (!UserAccess.CanChangeRoleOf(target, cmd.RequesterRole))
            return Result.Forbidden(UserAccess.ManageStaffForbiddenMessage);

        if (target.Role == newRole)
            return Result.Conflict($"El usuario ya tiene el rol '{cmd.NewRole}'.");

        target.ChangeRole(newRole);
        await _users.UpdateAsync(target, ct);

        return Result.Success();
    }
}
