namespace ELearning.Domain.Entities;

public class Course
{
    public Guid Id { get; private set; }
    public Guid CreatedBy { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsGlobal { get; private set; }
    public int? TimeLimitMins { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User CreatedByUser { get; private set; } = null!;
    public ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();
    public ICollection<CourseEnrollment> Enrollments { get; private set; } = new List<CourseEnrollment>();
    public ICollection<CourseCountry> CourseCountries { get; private set; } = new List<CourseCountry>();

    private Course() { }

    public static Course Create(string title, string? description, string? thumbnailUrl, Guid createdBy, bool isGlobal, int? timeLimitMins = null)
    {
        return new Course
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            ThumbnailUrl = thumbnailUrl,
            CreatedBy = createdBy,
            IsGlobal = isGlobal,
            TimeLimitMins = timeLimitMins,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string? description, string? thumbnailUrl, bool isGlobal, int? timeLimitMins)
    {
        Title = title;
        Description = description;
        ThumbnailUrl = thumbnailUrl;
        IsGlobal = isGlobal;
        TimeLimitMins = timeLimitMins;
        UpdatedAt = DateTime.UtcNow;
    }
}
