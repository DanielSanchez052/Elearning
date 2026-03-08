using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Users.Commands;

namespace ELearning.Application.Features.Users.Validators;

public sealed class ChangeUserRoleValidator : IValidator<ChangeUserRoleCommand>
{
    public ValidationResult Validate(ChangeUserRoleCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.TargetUserId == Guid.Empty)
            result.AddError(nameof(cmd.TargetUserId), "El id del usuario objetivo es requerido.");

        if (string.IsNullOrWhiteSpace(cmd.NewRole))
            result.AddError(nameof(cmd.NewRole), "El nuevo rol es requerido.");
        else if (!AllowedRoles.Contains(cmd.NewRole.ToLowerInvariant()))
            result.AddError(nameof(cmd.NewRole), "Rol inválido. Usa: student, instructor, admin, superadmin.");

        if (cmd.RequesterId == Guid.Empty)
            result.AddError(nameof(cmd.RequesterId), "El id del solicitante es requerido.");

        if (string.IsNullOrWhiteSpace(cmd.RequesterRole))
            result.AddError(nameof(cmd.RequesterRole), "El rol del solicitante es requerido.");

        return result;
    }

    private static readonly HashSet<string> AllowedRoles = new()
    {
        "student",
        "instructor",
        "admin",
        "superadmin"
    };
}
