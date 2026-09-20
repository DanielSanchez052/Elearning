using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetLessonQuizQuestionsAdminQuery(
    Guid LessonId
) : IQuery<IReadOnlyList<QuizQuestionAdminDto>>;

public sealed class GetLessonQuizQuestionsAdminHandler : IQueryHandler<GetLessonQuizQuestionsAdminQuery, IReadOnlyList<QuizQuestionAdminDto>>
{
    private readonly IQuizRepository _quizzes;

    public GetLessonQuizQuestionsAdminHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionAdminDto>>> HandleAsync(GetLessonQuizQuestionsAdminQuery query, CancellationToken ct = default)
    {
        if (query.LessonId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionAdminDto>>("LessonId es requerido");

        var questions = await _quizzes.GetQuestionsByLessonAsync(query.LessonId, ct);

        var quizDtos = questions
            .Select(q => new QuizQuestionAdminDto(
                q.Id,
                q.QuestionText,
                (int)q.Type,
                q.IsRequired,
                q.PassScore,
                q.MaxAttempts,
                q.OrderIndex,
                q.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select(o => new QuizOptionAdminDto(o.Id, o.OptionText, o.IsCorrect, o.OrderIndex))
                    .ToList(),
                q.LessonId,
                q.CourseId
            ))
            .ToList();

        return Result.Success<IReadOnlyList<QuizQuestionAdminDto>>(quizDtos);
    }
}
