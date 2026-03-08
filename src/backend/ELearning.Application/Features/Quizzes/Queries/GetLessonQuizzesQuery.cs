using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetLessonQuizzesQuery(
    Guid LessonId
) : IQuery<IReadOnlyList<QuizQuestionDto>>;

public sealed class GetLessonQuizzesHandler : IQueryHandler<GetLessonQuizzesQuery, IReadOnlyList<QuizQuestionDto>>
{
    private readonly IQuizRepository _quizzes;

    public GetLessonQuizzesHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionDto>>> HandleAsync(GetLessonQuizzesQuery query, CancellationToken ct = default)
    {
        if (query.LessonId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionDto>>("LessonId es requerido");

        var questions = await _quizzes.GetQuestionsByLessonAsync(query.LessonId, ct);

        var quizDtos = questions
            .Select(q => new QuizQuestionDto(
                q.Id,
                q.QuestionText,
                (int)q.Type,
                q.IsRequired,
                q.PassScore,
                q.MaxAttempts,
                q.OrderIndex,
                q.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select(o => new QuizOptionDto(o.Id, o.OptionText, o.OrderIndex))
                    .ToList(),
                q.LessonId,
                q.CourseId
            ))
            .ToList();

        return Result.Success<IReadOnlyList<QuizQuestionDto>>(quizDtos);
    }
}
