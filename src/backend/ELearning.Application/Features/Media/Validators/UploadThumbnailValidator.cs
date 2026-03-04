using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Media.Commands;
using ELearning.Application.Features.Media.constants;

namespace ELearning.Application.Features.Media.Validators;

public sealed class UploadThumbnailValidator : IValidator<UploadThumbnailCommand>
{
    public ValidationResult Validate(UploadThumbnailCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.File is null || cmd.File.Length == 0)
        {
            result.AddError(nameof(cmd.File), "La imagen del thumbnail es requerida.");
            return result;
        }

        if (cmd.File.Length > MediaLimits.ThumbnailMaxBytes)
            result.AddError(nameof(cmd.File),
                $"La imagen no puede superar {MediaLimits.ThumbnailMaxBytes / 1024 / 1024} MB.");

        if (!MediaLimits.ThumbnailContentTypes.Contains(cmd.File.ContentType.ToLowerInvariant()))
            result.AddError(nameof(cmd.File),
                "Solo se permiten imágenes JPEG, PNG o WebP.");

        return result;
    }
}
