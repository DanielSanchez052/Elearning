using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Auth.Commands;

namespace ELearning.Application.Features.Auth.Validators;

public sealed class VerifyEmailValidator : IValidator<VerifyEmailCommand>
{
    public ValidationResult Validate(VerifyEmailCommand cmd)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(cmd.Token))
            result.AddError(nameof(cmd.Token), "El token de verificación es requerido.");

        return result;
    }
}