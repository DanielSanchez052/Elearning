using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetCourseExamQuery(
    Guid CourseId
) : IQuery<IReadOnlyList<QuizQuestionDto>>;

public sealed class GetCourseExamHandler : IQueryHandler<GetCourseExamQuery, IReadOnlyList<QuizQuestionDto>>
{
    private readonly IQuizRepository _quizzes;

    public GetCourseExamHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionDto>>> HandleAsync(GetCourseExamQuery query, CancellationToken ct = default)
    {
        if (query.CourseId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionDto>>("CourseId es requerido");

        var questions = await _quizzes.GetQuestionsByCourseAsync(query.CourseId, ct);

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
