using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Common;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetLessonQuizQuestionsAdminQuery(
    Guid LessonId,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : IQuery<IReadOnlyList<QuizQuestionAdminDto>>;

public sealed class GetLessonQuizQuestionsAdminHandler : IQueryHandler<GetLessonQuizQuestionsAdminQuery, IReadOnlyList<QuizQuestionAdminDto>>
{
    private readonly IQuizRepository _quizzes;
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public GetLessonQuizQuestionsAdminHandler(IQuizRepository quizzes, ICourseRepository courses, ILessonRepository lessons)
    {
        _quizzes = quizzes;
        _courses = courses;
        _lessons = lessons;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionAdminDto>>> HandleAsync(GetLessonQuizQuestionsAdminQuery query, CancellationToken ct = default)
    {
        if (query.LessonId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionAdminDto>>("LessonId es requerido");

        // Incluye IsCorrect: solo quien puede gestionar el curso ve las respuestas
        if (!await QuizAuthoringAccess.CanManageLessonAsync(query.LessonId, query.RequesterId, query.RequesterRole, query.RequesterCountryId, _courses, _lessons, ct))
            return Result.Forbidden<IReadOnlyList<QuizQuestionAdminDto>>(QuizAuthoringAccess.ForbiddenMessage);

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
