using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Media.Commands;
using ELearning.Application.Features.Media.constants;

namespace ELearning.Application.Features.Media.Validators;

public sealed class UploadVideoValidator : IValidator<UploadVideoCommand>
{
    public ValidationResult Validate(UploadVideoCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.File is null || cmd.File.Length == 0)
        {
            result.AddError(nameof(cmd.File), "El archivo de video es requerido.");
            return result;
        }

        if (cmd.File.Length > MediaLimits.VideoMaxBytes)
            result.AddError(nameof(cmd.File),
                $"El video no puede superar {MediaLimits.VideoMaxBytes / 1024 / 1024} MB.");

        if (!MediaLimits.VideoContentTypes.Contains(cmd.File.ContentType.ToLowerInvariant()))
            result.AddError(nameof(cmd.File),
                "Solo se permiten archivos MP4.");

        return result;
    }
}
