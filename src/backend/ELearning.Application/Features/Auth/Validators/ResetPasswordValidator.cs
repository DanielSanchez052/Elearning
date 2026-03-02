using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Auth.Commands;

namespace ELearning.Application.Features.Auth.Validators;

public sealed class ResetPasswordValidator : IValidator<ResetPasswordCommand>
{
    public ValidationResult Validate(ResetPasswordCommand cmd)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(cmd.Token))
            result.AddError(nameof(cmd.Token), "El token es requerido.");

        if (string.IsNullOrWhiteSpace(cmd.NewPassword))
        {
            result.AddError(nameof(cmd.NewPassword), "La nueva contraseña es requerida.");
        }
        else
        {
            if (cmd.NewPassword.Length < 8)
                result.AddError(nameof(cmd.NewPassword), "Debe tener al menos 8 caracteres.");

            if (!cmd.NewPassword.Any(char.IsUpper))
                result.AddError(nameof(cmd.NewPassword), "Debe contener al menos una letra mayúscula.");

            if (!cmd.NewPassword.Any(char.IsLower))
                result.AddError(nameof(cmd.NewPassword), "Debe contener al menos una letra minúscula.");

            if (!cmd.NewPassword.Any(char.IsDigit))
                result.AddError(nameof(cmd.NewPassword), "Debe contener al menos un número.");
        }

        if (string.IsNullOrWhiteSpace(cmd.ConfirmPassword))
            result.AddError(nameof(cmd.ConfirmPassword), "La confirmación de contraseña es requerida.");
        else if (cmd.NewPassword != cmd.ConfirmPassword)
            result.AddError(nameof(cmd.ConfirmPassword), "Las contraseñas no coinciden.");

        return result;
    }
}