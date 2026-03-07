using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Enrollments.DTOs;

public record EnrollmentSummaryDto(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseTitle,
    string? CourseThumbnailUrl,
    EnrollmentStatus Status,
    int TotalLessons,
    int RequiredLessons,
    int CompletedLessons,
    int ProgressPercent,
    DateTime EnrolledAt,
    DateTime? CompletedAt,
    DateTime? DeadlineAt
);
