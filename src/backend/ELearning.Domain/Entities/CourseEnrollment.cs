using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class CourseEnrollment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DeadlineAt { get; private set; }

    public User User { get; private set; } = null!;
    public Course Course { get; private set; } = null!;
    public ICollection<UserLessonProgress> LessonProgress { get; private set; } = new List<UserLessonProgress>();

    private CourseEnrollment() { }

    public static CourseEnrollment Create(Guid userId, Guid courseId, DateTime? deadlineAt = null)
    {
        return new CourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow,
            DeadlineAt = deadlineAt,
        };
    }

    /// <summary>
    /// Marks the enrollment as completed if all required lessons are done.
    /// Returns false if there are still pending required lessons.
    /// </summary>
    public bool TryComplete(IEnumerable<Guid> requiredLessonIds)
    {
        var required = requiredLessonIds.ToHashSet();
        var completed = LessonProgress
            .Where(p => p.IsCompleted && required.Contains(p.LessonId))
            .Select(p => p.LessonId)
            .ToHashSet();

        if (!required.IsSubsetOf(completed))
            return false;

        Status = EnrollmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        return true;
    }

    public void Abandon()
    {
        Status = EnrollmentStatus.Abandoned;
    }

    public bool IsActive => Status == EnrollmentStatus.Active;
    public bool IsCompleted => Status == EnrollmentStatus.Completed;
}
