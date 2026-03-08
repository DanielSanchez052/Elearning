using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Quizzes.Commands;

namespace ELearning.Application.Features.Quizzes.Validators;

public sealed class UpdateQuizQuestionValidator : IValidator<UpdateQuizQuestionCommand>
{
    public ValidationResult Validate(UpdateQuizQuestionCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.QuestionId == Guid.Empty)
            result.AddError(nameof(cmd.QuestionId), "QuestionId es requerido");

        if (string.IsNullOrWhiteSpace(cmd.QuestionText))
            result.AddError(nameof(cmd.QuestionText), "La pregunta es requerida");
        else if (cmd.QuestionText.Length > 500)
            result.AddError(nameof(cmd.QuestionText), "La pregunta no puede exceder 500 caracteres");

        if (cmd.PassScore < 0 || cmd.PassScore > 100)
            result.AddError(nameof(cmd.PassScore), "PassScore debe estar entre 0 y 100");

        if (cmd.MaxAttempts <= 0)
            result.AddError(nameof(cmd.MaxAttempts), "MaxAttempts debe ser mayor a 0");

        return result;
    }
}
