using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record UpdateQuizQuestionCommand(
    Guid QuestionId,
    string QuestionText,
    decimal PassScore,
    int MaxAttempts,
    bool IsRequired
) : ICommand;

public sealed class UpdateQuizQuestionHandler : ICommandHandler<UpdateQuizQuestionCommand>
{
    private readonly IQuizRepository _quizzes;

    public UpdateQuizQuestionHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result> HandleAsync(UpdateQuizQuestionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.QuestionId == Guid.Empty)
            return Result.ValidationFailure("QuestionId es requerido");

        if (string.IsNullOrWhiteSpace(cmd.QuestionText))
            return Result.ValidationFailure("QuestionText es requerido");

        if (cmd.PassScore < 0 || cmd.PassScore > 100)
            return Result.ValidationFailure("PassScore debe estar entre 0 y 100");

        if (cmd.MaxAttempts <= 0)
            return Result.ValidationFailure("MaxAttempts debe ser mayor a 0");

        var question = await _quizzes.GetQuestionByIdAsync(cmd.QuestionId, ct);
        if (question is null)
            return Result.NotFound("Pregunta no encontrada");

        question.UpdateQuestion(cmd.QuestionText, cmd.PassScore, cmd.MaxAttempts, cmd.IsRequired);
        await _quizzes.UpdateQuestionAsync(question, ct);
        await _quizzes.SaveChangesAsync(ct);

        return Result.Success();
    }
}
