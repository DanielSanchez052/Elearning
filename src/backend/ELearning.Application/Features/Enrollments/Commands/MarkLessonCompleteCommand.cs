using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Exceptions;
using ELearning.Application.Features.Enrollments.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Enrollments.Commands;

public record MarkLessonCompleteCommand(
    Guid UserId,
    Guid CourseId,
    Guid LessonId
) : ICommand<MarkLessonCompleteResult>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class MarkLessonCompleteHandler : ICommandHandler<MarkLessonCompleteCommand, MarkLessonCompleteResult>
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IQuizRepository _quizzes;

    public MarkLessonCompleteHandler(IEnrollmentRepository enrollments, IQuizRepository quizzes)
    {
        _enrollments = enrollments;
        _quizzes = quizzes;
    }

    public async Task<Result<MarkLessonCompleteResult>> HandleAsync(
        MarkLessonCompleteCommand command, CancellationToken ct = default)
    {
        var enrollment = await _enrollments.GetByUserAndCourseAsync(command.UserId, command.CourseId, ct)
            ?? throw new NotFoundException("Enrollment not found for this user and course.");

        if(enrollment is null)
        {
            return Result.NotFound<MarkLessonCompleteResult>("el usuario no esta inscrito en el curso indicado.");
        }

        if (!enrollment.IsActive)
            return Result.Conflict<MarkLessonCompleteResult>("el usuario no tiene una inscripción activa en el curso indicado.");

        var course = enrollment.Course;
        var lesson = course.Lessons.FirstOrDefault(l => l.Id == command.LessonId);

        if(lesson is null)
        {
            return Result.NotFound<MarkLessonCompleteResult>("la lección indicada no existe en el curso.");
        }

        // 3. Get or create progress record
        var progress = await _enrollments.GetProgressAsync(enrollment.Id, command.LessonId, ct);
        bool wasAlreadyComplete = false;

        if (progress is null)
        {
            progress = UserLessonProgress.Create(enrollment.Id, command.LessonId);
            await _enrollments.AddProgressAsync(progress, ct);
        }
        else
        {
            wasAlreadyComplete = progress.IsCompleted;
        }

        progress.MarkComplete();

        var allProgress = enrollment.LessonProgress.ToList();
        if (!allProgress.Any(p => p.LessonId == command.LessonId))
            allProgress.Add(progress);
        else
        {
            var existing = allProgress.First(p => p.LessonId == command.LessonId);
        }

        var requiredLessonIds = course.Lessons
            .Where(l => l.IsRequired)
            .Select(l => l.Id)
            .ToList();

        var completedRequired = allProgress
            .Count(p => p.IsCompleted && requiredLessonIds.Contains(p.LessonId));

        var hasFinalExam = (await _quizzes.GetQuestionsByCourseAsync(command.CourseId, ct)).Count > 0;
        var allRequiredLessonsCompleted = completedRequired >= requiredLessonIds.Count;

        bool courseCompleted = false;
        if (allRequiredLessonsCompleted && !hasFinalExam)
        {
            courseCompleted = enrollment.TryComplete(requiredLessonIds);
        }

        await _enrollments.SaveChangesAsync(ct);

        return new MarkLessonCompleteResult(
            LessonWasAlreadyComplete: wasAlreadyComplete,
            CourseCompleted:          courseCompleted,
            CompletedLessons:         completedRequired,
            TotalRequiredLessons:     requiredLessonIds.Count
        );
    }
}
