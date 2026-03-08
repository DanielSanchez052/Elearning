using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Quizzes.Validators;

public sealed class CreateQuizQuestionValidator : IValidator<CreateQuizQuestionCommand>
{
    public ValidationResult Validate(CreateQuizQuestionCommand cmd)
    {
        var result = new ValidationResult();

        // Validar exactamente una de las dos FK
        var hasLesson = cmd.LessonId.HasValue && cmd.LessonId != Guid.Empty;
        var hasCourse = cmd.CourseId.HasValue && cmd.CourseId != Guid.Empty;

        if (!hasLesson && !hasCourse)
            result.AddError("FK", "Debe proporcionar LessonId o CourseId");
        else if (hasLesson && hasCourse)
            result.AddError("FK", "No puede proporcionar ambos LessonId y CourseId");

        // Validar Type
        if (cmd.Type != (int)QuizType.PerLesson && cmd.Type != (int)QuizType.CourseExam)
            result.AddError(nameof(cmd.Type), "Type debe ser PerLesson (0) o CourseExam (1)");

        // Validar QuestionText
        if (string.IsNullOrWhiteSpace(cmd.QuestionText))
            result.AddError(nameof(cmd.QuestionText), "La pregunta es requerida");
        else if (cmd.QuestionText.Length > 500)
            result.AddError(nameof(cmd.QuestionText), "La pregunta no puede exceder 500 caracteres");

        // Validar PassScore
        if (cmd.PassScore < 0 || cmd.PassScore > 100)
            result.AddError(nameof(cmd.PassScore), "PassScore debe estar entre 0 y 100");

        // Validar MaxAttempts
        if (cmd.MaxAttempts <= 0)
            result.AddError(nameof(cmd.MaxAttempts), "MaxAttempts debe ser mayor a 0");

        return result;
    }
}
