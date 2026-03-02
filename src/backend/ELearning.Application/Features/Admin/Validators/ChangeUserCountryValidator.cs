using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Admin.Commands;

namespace ELearning.Application.Features.Admin.Validators;

public sealed class ChangeUserCountryValidator : IValidator<ChangeUserCountryCommand>
{
    public ValidationResult Validate(ChangeUserCountryCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.TargetUserId == Guid.Empty)
            result.AddError(nameof(cmd.TargetUserId), "El usuario destino es requerido.");

        if (cmd.NewCountryId <= 0)
            result.AddError(nameof(cmd.NewCountryId), "El país destino es requerido.");

        return result;
    }
}