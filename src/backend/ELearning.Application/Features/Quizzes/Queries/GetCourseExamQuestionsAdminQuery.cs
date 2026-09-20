using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetCourseExamQuestionsAdminQuery(
    Guid CourseId
) : IQuery<IReadOnlyList<QuizQuestionAdminDto>>;

public sealed class GetCourseExamQuestionsAdminHandler : IQueryHandler<GetCourseExamQuestionsAdminQuery, IReadOnlyList<QuizQuestionAdminDto>>
{
    private readonly IQuizRepository _quizzes;

    public GetCourseExamQuestionsAdminHandler(IQuizRepository quizzes)
    {
        _quizzes = quizzes;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionAdminDto>>> HandleAsync(GetCourseExamQuestionsAdminQuery query, CancellationToken ct = default)
    {
        if (query.CourseId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionAdminDto>>("CourseId es requerido");

        var questions = await _quizzes.GetQuestionsByCourseAsync(query.CourseId, ct);

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
