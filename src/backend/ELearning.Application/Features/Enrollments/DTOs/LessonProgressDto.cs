namespace ELearning.Application.Features.Enrollments.DTOs;

public record LessonProgressDto(
    Guid LessonId,
    string Title,
    string Type,
    int OrderIndex,
    bool IsRequired,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime? LastAccessedAt
);
