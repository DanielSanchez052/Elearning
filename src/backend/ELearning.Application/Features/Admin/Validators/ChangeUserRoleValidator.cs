using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Admin.Commands;

namespace ELearning.Application.Features.Admin.Validators;

public sealed class ChangeUserRoleValidator : IValidator<ChangeUserRoleCommand>
{
    // Roles válidos que pueden asignarse
    private static readonly string[] ValidRoles = ["student", "instructor", "admin", "super_admin"];

    public ValidationResult Validate(ChangeUserRoleCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.TargetUserId == Guid.Empty)
            result.AddError(nameof(cmd.TargetUserId), "El usuario destino es requerido.");

        if (string.IsNullOrWhiteSpace(cmd.NewRole))
        {
            result.AddError(nameof(cmd.NewRole), "El nuevo rol es requerido.");
            return result;
        }

        if (!ValidRoles.Contains(cmd.NewRole.ToLowerInvariant()))
            result.AddError(nameof(cmd.NewRole),
                $"Rol inválido. Los roles permitidos son: {string.Join(", ", ValidRoles)}.");

        return result;
    }
}