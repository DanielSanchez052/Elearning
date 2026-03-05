using ELearning.Application.Features.Countries.Dtos;

namespace ELearning.Application.Features.Courses.DTOs;

public sealed record CourseDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsGlobal,
    bool IsActive,
    string InstructorName,
    Guid InstructorId,
    IReadOnlyList<LessonDto> Lessons,
    IReadOnlyList<CountryDto> Countries,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
