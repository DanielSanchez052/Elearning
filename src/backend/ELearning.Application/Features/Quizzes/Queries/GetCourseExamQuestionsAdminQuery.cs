using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Common;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetCourseExamQuestionsAdminQuery(
    Guid CourseId,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : IQuery<IReadOnlyList<QuizQuestionAdminDto>>;

public sealed class GetCourseExamQuestionsAdminHandler : IQueryHandler<GetCourseExamQuestionsAdminQuery, IReadOnlyList<QuizQuestionAdminDto>>
{
    private readonly IQuizRepository _quizzes;
    private readonly ICourseRepository _courses;

    public GetCourseExamQuestionsAdminHandler(IQuizRepository quizzes, ICourseRepository courses)
    {
        _quizzes = quizzes;
        _courses = courses;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionAdminDto>>> HandleAsync(GetCourseExamQuestionsAdminQuery query, CancellationToken ct = default)
    {
        if (query.CourseId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionAdminDto>>("CourseId es requerido");

        // Incluye IsCorrect: solo quien puede gestionar el curso ve las respuestas
        if (!await QuizAuthoringAccess.CanManageCourseAsync(query.CourseId, query.RequesterId, query.RequesterRole, query.RequesterCountryId, _courses, ct))
            return Result.Forbidden<IReadOnlyList<QuizQuestionAdminDto>>(QuizAuthoringAccess.ForbiddenMessage);

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
