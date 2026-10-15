namespace ELearning.Application.Features.Courses.DTOs;

public sealed record CourseSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsGlobal,
    bool IsActive,
    Guid? InstructorId,
    string? InstructorName,
    int LessonCount,
    DateTime CreatedAt
);
