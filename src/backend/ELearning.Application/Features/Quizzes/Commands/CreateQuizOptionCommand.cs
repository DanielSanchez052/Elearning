using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record CreateQuizOptionCommand(
    Guid QuestionId,
    string OptionText,
    bool IsCorrect,
    int OrderIndex
) : ICommand<Guid>;

public sealed class CreateQuizOptionHandler : ICommandHandler<CreateQuizOptionCommand, Guid>
{
    private readonly IQuizRepository _quizzes;

    public CreateQuizOptionHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result<Guid>> HandleAsync(CreateQuizOptionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.QuestionId == Guid.Empty)
            return Result.ValidationFailure<Guid>("QuestionId es requerido");

        if (string.IsNullOrWhiteSpace(cmd.OptionText))
            return Result.ValidationFailure<Guid>("OptionText es requerido");

        if (cmd.OrderIndex <= 0)
            return Result.ValidationFailure<Guid>("OrderIndex debe ser mayor a 0");

        var question = await _quizzes.GetQuestionByIdAsync(cmd.QuestionId, ct);
        if (question is null)
            return Result.NotFound<Guid>("Pregunta no encontrada");

        var option = QuizOption.Create(cmd.QuestionId, cmd.OptionText, cmd.IsCorrect, cmd.OrderIndex);
        await _quizzes.CreateOptionAsync(option, ct);
        await _quizzes.SaveChangesAsync(ct);

        return option.Id;
    }
}
