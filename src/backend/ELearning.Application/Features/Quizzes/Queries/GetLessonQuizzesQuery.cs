using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Queries;

public sealed record GetLessonQuizzesQuery(
    Guid UserId,
    Guid LessonId
) : IQuery<IReadOnlyList<QuizQuestionDto>>;

public sealed class GetLessonQuizzesHandler : IQueryHandler<GetLessonQuizzesQuery, IReadOnlyList<QuizQuestionDto>>
{
    private readonly IQuizRepository _quizzes;
    private readonly ILessonRepository _lessons;
    private readonly IEnrollmentRepository _enrollments;

    public GetLessonQuizzesHandler(
        IQuizRepository quizzes,
        ILessonRepository lessons,
        IEnrollmentRepository enrollments)
    {
        _quizzes = quizzes;
        _lessons = lessons;
        _enrollments = enrollments;
    }

    public async Task<Result<IReadOnlyList<QuizQuestionDto>>> HandleAsync(GetLessonQuizzesQuery query, CancellationToken ct = default)
    {
        if (query.UserId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionDto>>("UserId es requerido");

        if (query.LessonId == Guid.Empty)
            return Result.ValidationFailure<IReadOnlyList<QuizQuestionDto>>("LessonId es requerido");

        var lesson = await _lessons.GetByIdAsync(query.LessonId, ct);
        if (lesson is null)
            return Result.NotFound<IReadOnlyList<QuizQuestionDto>>("Lección no encontrada");

        var enrollment = await _enrollments.GetByUserAndCourseAsync(query.UserId, lesson.CourseId, ct);
        if (enrollment is null)
            return Result.Forbidden<IReadOnlyList<QuizQuestionDto>>("No estás inscrito en este curso");

        if (!enrollment.IsActive)
            return Result.Forbidden<IReadOnlyList<QuizQuestionDto>>("No tienes una inscripción activa en este curso");

        var completedRequiredIds = enrollment.LessonProgress
            .Where(p => p.IsCompleted)
            .Select(p => p.LessonId)
            .ToHashSet();

        var missingRequiredBeforeLesson = enrollment.Course.Lessons
            .Where(l => l.IsRequired && l.OrderIndex < lesson.OrderIndex)
            .Select(l => l.Id)
            .Where(id => !completedRequiredIds.Contains(id))
            .ToList();

        if (missingRequiredBeforeLesson.Count > 0)
            return Result.Forbidden<IReadOnlyList<QuizQuestionDto>>(
                "Debes completar las lecciones requeridas previas antes de presentar esta evaluación.");

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
