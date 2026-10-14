using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Common;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record UpdateQuizOptionCommand(
    Guid OptionId,
    string OptionText,
    bool IsCorrect,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : ICommand;

public sealed class UpdateQuizOptionHandler : ICommandHandler<UpdateQuizOptionCommand>
{
    private readonly IQuizRepository _quizzes;
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public UpdateQuizOptionHandler(IQuizRepository quizzes, ICourseRepository courses, ILessonRepository lessons)
    {
        _quizzes = quizzes;
        _courses = courses;
        _lessons = lessons;
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

        if (!await QuizAuthoringAccess.CanManageQuestionAsync(option.QuestionId, cmd.RequesterId, cmd.RequesterRole, cmd.RequesterCountryId, _quizzes, _courses, _lessons, ct))
            return Result.Forbidden(QuizAuthoringAccess.ForbiddenMessage);

        option.Update(cmd.OptionText, cmd.IsCorrect);

        await _quizzes.UpdateOptionAsync(option, ct);
        await _quizzes.SaveChangesAsync(ct);

        return Result.Success();
    }
}
