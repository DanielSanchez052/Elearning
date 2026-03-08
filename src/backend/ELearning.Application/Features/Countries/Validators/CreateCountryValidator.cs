using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Countries.Commands;

namespace ELearning.Application.Features.Countries.Validators;

public sealed class CreateCountryValidator : IValidator<CreateCountryCommand>
{
    public ValidationResult Validate(CreateCountryCommand cmd)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(cmd.Code))
            result.AddError(nameof(cmd.Code), "El código del país es requerido.");
        else if (cmd.Code.Length != 3)
            result.AddError(nameof(cmd.Code), "El código debe tener exactamente 3 caracteres (ISO 3166-1 alpha-3). Ej: COL, MEX, ARG.");
        else if (!cmd.Code.All(char.IsLetter))
            result.AddError(nameof(cmd.Code), "El código solo puede contener letras.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            result.AddError(nameof(cmd.Name), "El nombre del país es requerido.");
        else if (cmd.Name.Length > 100)
            result.AddError(nameof(cmd.Name), "El nombre no puede superar 100 caracteres.");

        return result;
    }
}
