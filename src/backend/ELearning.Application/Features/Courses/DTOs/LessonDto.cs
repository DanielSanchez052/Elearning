namespace ELearning.Application.Features.Courses.DTOs;

public sealed record LessonDto(
    Guid Id,
    string Title,
    string Type,
    string? ContentUrl,
    int OrderIndex,
    bool IsRequired
);
