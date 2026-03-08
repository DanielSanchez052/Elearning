using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record DeleteQuizOptionCommand(
    Guid OptionId
) : ICommand;

public sealed class DeleteQuizOptionHandler : ICommandHandler<DeleteQuizOptionCommand>
{
    private readonly IQuizRepository _quizzes;

    public DeleteQuizOptionHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result> HandleAsync(DeleteQuizOptionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.OptionId == Guid.Empty)
            return Result.ValidationFailure("OptionId es requerido");

        var option = await _quizzes.GetOptionByIdAsync(cmd.OptionId, ct);
        if (option is null)
            return Result.NotFound("Opción no encontrada");

        await _quizzes.DeleteOptionAsync(cmd.OptionId, ct);
        await _quizzes.SaveChangesAsync(ct);

        return Result.Success();
    }
}
