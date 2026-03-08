using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record UpdateQuizOptionCommand(
    Guid OptionId,
    string OptionText,
    bool IsCorrect
) : ICommand;

public sealed class UpdateQuizOptionHandler : ICommandHandler<UpdateQuizOptionCommand>
{
    private readonly IQuizRepository _quizzes;

    public UpdateQuizOptionHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result> HandleAsync(UpdateQuizOptionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.OptionId == Guid.Empty)
            return Result.ValidationFailure("OptionId es requerido");

        if (string.IsNullOrWhiteSpace(cmd.OptionText))
            return Result.ValidationFailure("OptionText es requerido");

        var option = await _quizzes.GetOptionByIdAsync(cmd.OptionId, ct);
        if (option is null)
            return Result.NotFound("Opción no encontrada");

        option.Update(cmd.OptionText, cmd.IsCorrect);

        await _quizzes.UpdateOptionAsync(option, ct);
        await _quizzes.SaveChangesAsync(ct);

        return Result.Success();
    }
}
