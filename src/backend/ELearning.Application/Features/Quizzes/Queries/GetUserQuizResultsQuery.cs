using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetUserQuizResultsQuery(
    Guid UserId,
    Guid? LessonId,
    Guid? CourseId
) : IQuery<IReadOnlyList<QuizAttemptDto>>;

public sealed class GetUserQuizResultsHandler : IQueryHandler<GetUserQuizResultsQuery, IReadOnlyList<QuizAttemptDto>>
{
    private readonly IQuizRepository _quizzes;

    public GetUserQuizResultsHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result<IReadOnlyList<QuizAttemptDto>>> HandleAsync(GetUserQuizResultsQuery query, CancellationToken ct = default)
    {
        if (query.UserId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizAttemptDto>>("UserId es requerido");

        if ((query.LessonId == null || query.LessonId == Guid.Empty) &&
            (query.CourseId == null || query.CourseId == Guid.Empty))
            return Result.ValidationFailure<IReadOnlyList<QuizAttemptDto>>("Debe proporcionar LessonId o CourseId");

        IReadOnlyList<UserQuizResult> results;

        if (query.LessonId.HasValue && query.LessonId != Guid.Empty)
        {
            results = await _quizzes.GetLessonAttemptsAsync(query.UserId, query.LessonId.Value, ct);
        }
        else
        {
            results = await _quizzes.GetCourseExamAttemptsAsync(query.UserId, query.CourseId!.Value, ct);
        }

        var attempts = results
            .Select(r => new QuizAttemptDto(
                r.AttemptNumber,
                r.Score,
                r.IsPassed,
                r.CompletedAt
            ))
            .ToList();

        return Result.Success<IReadOnlyList<QuizAttemptDto>>(attempts);
    }
}
