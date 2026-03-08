using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Quizzes.Commands;

namespace ELearning.Application.Features.Quizzes.Validators;

public sealed class SubmitQuizValidator : IValidator<SubmitQuizCommand>
{
    public ValidationResult Validate(SubmitQuizCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.UserId == Guid.Empty)
            result.AddError(nameof(cmd.UserId), "UserId es requerido");

        // Validar exactamente una de las dos FK
        var hasLesson = cmd.LessonId.HasValue && cmd.LessonId != Guid.Empty;
        var hasCourse = cmd.CourseId.HasValue && cmd.CourseId != Guid.Empty;

        if (!hasLesson && !hasCourse)
            result.AddError("FK", "Debe proporcionar LessonId o CourseId");
        else if (hasLesson && hasCourse)
            result.AddError("FK", "No puede proporcionar ambos LessonId y CourseId");

        if (cmd.SelectedOptionIds == null || cmd.SelectedOptionIds.Count == 0)
            result.AddError(nameof(cmd.SelectedOptionIds), "Debe seleccionar al menos una opción");
        else
        {
            var emptyIds = cmd.SelectedOptionIds.Where(id => id == Guid.Empty).ToList();
            if (emptyIds.Count != 0)
                result.AddError(nameof(cmd.SelectedOptionIds), "Todas las opciones seleccionadas deben tener IDs válidos");
        }

        return result;
    }
}
