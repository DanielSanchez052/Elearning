namespace ELearning.Domain.Entities;

public class UserLessonProgress
{
    public Guid Id { get; private set; }
    public Guid EnrollmentId { get; private set; }
    public Guid LessonId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime LastAccessedAt { get; private set; }

    public CourseEnrollment Enrollment { get; private set; } = null!;
    public Lesson Lesson { get; private set; } = null!;

    private UserLessonProgress() { }

    public static UserLessonProgress Create(Guid enrollmentId, Guid lessonId)
    {
        return new UserLessonProgress
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollmentId,
            LessonId = lessonId,
            IsCompleted = false,
            LastAccessedAt = DateTime.UtcNow,
        };
    }

    public void MarkComplete()
    {
        if (IsCompleted) return;
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        LastAccessedAt = DateTime.UtcNow;
    }

    public void RecordAccess()
    {
        LastAccessedAt = DateTime.UtcNow;
    }
}
