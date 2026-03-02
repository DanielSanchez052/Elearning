namespace ELearning.Application.Features.Courses.DTOs;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsGlobal { get; set; }
    public int? TimeLimitMins { get; set; }
    public int LessonsCount { get; set; }
    public List<int> CountryIds { get; set; } = new();
}

public class CourseDetailDto : CourseDto
{
    public List<LessonDto> Lessons { get; set; } = new();
}

public class LessonDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ContentUrl { get; set; }
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; }
}

public class CreateCourseRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsGlobal { get; set; }
    public int? TimeLimitMins { get; set; }
    public List<int> CountryIds { get; set; } = new();
}

public class UpdateCourseRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsGlobal { get; set; }
    public int? TimeLimitMins { get; set; }
    public List<int> CountryIds { get; set; } = new();
}
