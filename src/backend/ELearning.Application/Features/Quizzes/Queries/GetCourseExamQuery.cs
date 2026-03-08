using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetCourseExamQuery(
    Guid UserId,
    Guid CourseId
) : IQuery<IReadOnlyList<QuizQuestionDto>>;

public sealed class GetCourseExamHandler : IQueryHandler<GetCourseExamQuery, IReadOnlyList<QuizQuestionDto>>
{
    private readonly IQuizRepository _quizzes;
    private readonly IEnrollmentRepository _enrollments;

    public GetCourseExamHandler(IQuizRepository quizzes, IEnrollmentRepository enrollments)
    {
        _quizzes = quizzes;
        _enrollments = enrollments;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionDto>>> HandleAsync(GetCourseExamQuery query, CancellationToken ct = default)
    {
        if (query.UserId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionDto>>("UserId es requerido");

        if (query.CourseId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionDto>>("CourseId es requerido");

        var enrollment = await _enrollments.GetByUserAndCourseAsync(query.UserId, query.CourseId, ct);
        if (enrollment is null)
            return Result.Forbidden<IReadOnlyList<QuizQuestionDto>>("No estás inscrito en este curso");

        if (!enrollment.IsActive)
            return Result.Forbidden<IReadOnlyList<QuizQuestionDto>>("No tienes una inscripción activa en este curso");

        var completedRequiredIds = enrollment.LessonProgress
            .Where(p => p.IsCompleted)
            .Select(p => p.LessonId)
            .ToHashSet();

        var missingRequiredLessons = enrollment.Course.Lessons
            .Where(l => l.IsRequired)
            .Select(l => l.Id)
            .Where(id => !completedRequiredIds.Contains(id))
            .ToList();

        if (missingRequiredLessons.Count > 0)
            return Result.Forbidden<IReadOnlyList<QuizQuestionDto>>(
                "Debes completar todas las lecciones requeridas antes de presentar el examen final.");

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
