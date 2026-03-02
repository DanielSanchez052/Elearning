using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Validators;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Admin.Commands;

public sealed record ChangeUserRoleCommand(
    Guid TargetUserId,   // usuario al que se le cambia el rol
    string NewRole,        // rol como string — se valida en el validator
    Guid RequesterId,    // quien hace la operación (extraído del JWT)
    string RequesterRole   // rol del que hace la operación (extraído del JWT)
) : ICommand;


public sealed class ChangeUserRoleHandler : ICommandHandler<ChangeUserRoleCommand>
{
    private readonly IUserRepository _users;

    public ChangeUserRoleHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result> HandleAsync(ChangeUserRoleCommand cmd, CancellationToken ct = default)
    {
        var newRole = Enum.Parse<UserRole>(cmd.NewRole, ignoreCase: true);

        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);

        if (requesterRole == UserRole.Admin &&
            newRole is not (UserRole.Student or UserRole.Instructor))
            return Result.Forbidden(
                "Un Admin solo puede asignar los roles Student e Instructor.");

        var target = await _users.GetByIdTrackedAsync(cmd.TargetUserId, ct);
        if (target is null)
            return Result.NotFound($"Usuario con id '{cmd.TargetUserId}' no encontrado.");

        if (target.Role == UserRole.SuperAdmin && requesterRole != UserRole.SuperAdmin)
            return Result.Forbidden("No puedes modificar el rol de un Super Admin.");

        if (target.Id == cmd.RequesterId)
            return Result.Forbidden("No puedes cambiar tu propio rol.");

        if (target.Role == newRole)
            return Result.Conflict($"El usuario ya tiene el rol '{cmd.NewRole}'.");

        target.ChangeRole(newRole);
        await _users.UpdateAsync(target, ct);

        return Result.Success();
    }
}