using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Lessons.Commands;

namespace ELearning.Application.Features.Lessons.Validators;

public sealed class UpdateLessonValidator : IValidator<UpdateLessonCommand>
{
    public ValidationResult Validate(UpdateLessonCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.LessonId == Guid.Empty)
            result.AddError(nameof(cmd.LessonId), "La lección es requerida.");

        if (string.IsNullOrWhiteSpace(cmd.Title))
            result.AddError(nameof(cmd.Title), "El título de la lección es requerido.");
        else if (cmd.Title.Length > 200)
            result.AddError(nameof(cmd.Title), "El título no puede superar 200 caracteres.");

        return result;
    }
}
