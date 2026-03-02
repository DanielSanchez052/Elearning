using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Auth.Commands;

namespace ELearning.Application.Features.Auth.Validators;

public sealed class LoginValidator : IValidator<LoginCommand>
{
    public ValidationResult Validate(LoginCommand cmd)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(cmd.Email))
            result.AddError(nameof(cmd.Email), "El email es requerido.");

        if (string.IsNullOrWhiteSpace(cmd.Password))
            result.AddError(nameof(cmd.Password), "La contraseña es requerida.");

        return result;
    }
}
