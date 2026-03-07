namespace ELearning.Application.Features.Enrollments.DTOs;

public record MarkLessonCompleteResult(
    bool LessonWasAlreadyComplete,
    bool CourseCompleted,
    int CompletedLessons,
    int TotalRequiredLessons
);
