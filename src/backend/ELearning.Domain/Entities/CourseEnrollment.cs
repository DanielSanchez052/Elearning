namespace ELearning.Domain.Entities;

public class CourseEnrollment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public User User { get; private set; } = null!;
    public Course Course { get; private set; } = null!;
    public ICollection<UserLessonProgress> LessonProgress { get; private set; } = new List<UserLessonProgress>();

    private CourseEnrollment() { }

    public static CourseEnrollment Create(Guid userId, Guid courseId)
    {
        return new CourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        };
    }

    public void Start()
    {
        if (StartedAt == null)
        {
            StartedAt = DateTime.UtcNow;
        }
    }

    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
    }

    public bool IsCompleted => CompletedAt.HasValue;
}
