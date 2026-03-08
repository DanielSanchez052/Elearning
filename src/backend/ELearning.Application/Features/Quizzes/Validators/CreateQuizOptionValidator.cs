using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Quizzes.Commands;

namespace ELearning.Application.Features.Quizzes.Validators;

public sealed class CreateQuizOptionValidator : IValidator<CreateQuizOptionCommand>
{
    public ValidationResult Validate(CreateQuizOptionCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.QuestionId == Guid.Empty)
            result.AddError(nameof(cmd.QuestionId), "QuestionId es requerido");

        if (string.IsNullOrWhiteSpace(cmd.OptionText))
            result.AddError(nameof(cmd.OptionText), "La opción es requerida");
        else if (cmd.OptionText.Length > 200)
            result.AddError(nameof(cmd.OptionText), "La opción no puede exceder 200 caracteres");

        if (cmd.OrderIndex <= 0)
            result.AddError(nameof(cmd.OrderIndex), "OrderIndex debe ser mayor a 0");

        return result;
    }
}
