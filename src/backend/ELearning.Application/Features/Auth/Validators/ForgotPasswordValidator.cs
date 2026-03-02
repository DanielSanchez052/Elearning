using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Auth.Commands;

namespace ELearning.Application.Features.Auth.Validators;


public sealed class ForgotPasswordValidator : IValidator<ForgotPasswordCommand>
{
    public ValidationResult Validate(ForgotPasswordCommand cmd)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(cmd.Email))
            result.AddError(nameof(cmd.Email), "El email es requerido.");

        return result;
    }
}
