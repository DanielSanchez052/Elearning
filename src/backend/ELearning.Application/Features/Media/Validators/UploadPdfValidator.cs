using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Media.Commands;
using ELearning.Application.Features.Media.constants;

namespace ELearning.Application.Features.Media.Validators;

public sealed class UploadPdfValidator : IValidator<UploadPdfCommand>
{
    public ValidationResult Validate(UploadPdfCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.File is null || cmd.File.Length == 0)
        {
            result.AddError(nameof(cmd.File), "El archivo PDF es requerido.");
            return result;
        }

        if (cmd.File.Length > MediaLimits.DocumentMaxBytes)
            result.AddError(nameof(cmd.File),
                $"El PDF no puede superar {MediaLimits.DocumentMaxBytes / 1024 / 1024} MB.");

        if (!MediaLimits.DocumentContentTypes.Contains(cmd.File.ContentType.ToLowerInvariant()))
            result.AddError(nameof(cmd.File),
                "Solo se permiten archivos PDF.");

        return result;
    }
}
