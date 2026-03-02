namespace ELearning.Application.Features.Lessons.DTOs;

public class LessonDto { public Guid Id { get; set; } public string Title { get; set; } = ""; public string Type { get; set; } = ""; public string? ContentUrl { get; set; } public int OrderIndex { get; set; } public bool IsRequired { get; set; } }
public class CreateLessonDto { public string Title { get; set; } = ""; public string Type { get; set; } = ""; public string? ContentUrl { get; set; } public int OrderIndex { get; set; } public bool IsRequired { get; set; } }
