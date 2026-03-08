using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record DeleteQuizQuestionCommand(
    Guid QuestionId
) : ICommand;

public sealed class DeleteQuizQuestionHandler : ICommandHandler<DeleteQuizQuestionCommand>
{
    private readonly IQuizRepository _quizzes;

    public DeleteQuizQuestionHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result> HandleAsync(DeleteQuizQuestionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.QuestionId == Guid.Empty)
            return Result.ValidationFailure("QuestionId es requerido");

        var question = await _quizzes.GetQuestionByIdAsync(cmd.QuestionId, ct);
        if (question is null)
            return Result.NotFound("Pregunta no encontrada");

        await _quizzes.DeleteQuestionAsync(cmd.QuestionId, ct);
        await _quizzes.SaveChangesAsync(ct);

        return Result.Success();
    }
}
