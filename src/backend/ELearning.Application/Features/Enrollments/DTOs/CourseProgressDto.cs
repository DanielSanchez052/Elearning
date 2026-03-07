using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Enrollments.DTOs;

public record CourseProgressDto(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseTitle,
    string? CourseThumbnailUrl,
    EnrollmentStatus Status,
    int ProgressPercent,
    int CompletedLessons,
    int RequiredLessons,
    DateTime EnrolledAt,
    DateTime? CompletedAt,
    IReadOnlyList<LessonProgressDto> Lessons
);
