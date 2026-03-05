using ELearning.Application.Features.Lessons.Commands;

namespace ELearning.API.Models;

public sealed record CreateCourseRequest(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsGlobal
);

public sealed record UpdateCourseRequest(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsGlobal
);

public sealed record AssignCountriesRequest(List<int> CountryIds);

public sealed record CreateLessonRequest(
    string Title,
    string Type,
    string? ContentUrl,
    bool IsRequired
);

public sealed record UpdateLessonRequest(
    string Title,
    string? ContentUrl,
    bool IsRequired
);

public sealed record ReorderLessonsRequest(List<LessonOrderItem> Orders);
