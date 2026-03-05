using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Lessons.Commands;

namespace ELearning.Application.Features.Lessons.Validators;

public sealed class CreateLessonValidator : IValidator<CreateLessonCommand>
{
    private static readonly string[] ValidTypes = ["video", "pdf", "quiz"];

    public ValidationResult Validate(CreateLessonCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.CourseId == Guid.Empty)
            result.AddError(nameof(cmd.CourseId), "El curso es requerido.");

        if (string.IsNullOrWhiteSpace(cmd.Title))
            result.AddError(nameof(cmd.Title), "El título de la lección es requerido.");
        else if (cmd.Title.Length > 200)
            result.AddError(nameof(cmd.Title), "El título no puede superar 200 caracteres.");

        if (string.IsNullOrWhiteSpace(cmd.Type))
            result.AddError(nameof(cmd.Type), "El tipo de lección es requerido.");
        else if (!ValidTypes.Contains(cmd.Type.ToLowerInvariant()))
            result.AddError(nameof(cmd.Type),
                $"Tipo inválido. Los tipos permitidos son: {string.Join(", ", ValidTypes)}.");

        // ContentUrl requerido para video y pdf, no para quiz
        if (cmd.Type?.ToLowerInvariant() is "video" or "pdf"
            && string.IsNullOrWhiteSpace(cmd.ContentUrl))
            result.AddError(nameof(cmd.ContentUrl),
                "La URL del contenido es requerida para lecciones de tipo video o PDF.");

        return result;
    }
}