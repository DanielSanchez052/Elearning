namespace ELearning.Domain.Entities;

public class UserQuizResult
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? LessonId { get; private set; }
    public Guid? CourseId { get; private set; }
    public int AttemptNumber { get; private set; }
    public decimal Score { get; private set; }
    public bool IsPassed { get; private set; }
    public DateTime CompletedAt { get; private set; }

    /// <summary>
    /// When the attempt was explicitly started (course exams started through an ExamSession).
    /// Null for lesson quizzes and for submissions made without a start call.
    /// </summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>Time taken on the attempt, or null when the start time is unknown.</summary>
    public TimeSpan? Duration =>
        StartedAt is null || CompletedAt == default ? null : CompletedAt - StartedAt.Value;

    public User User { get; private set; } = null!;
    public Lesson? Lesson { get; private set; }
    public Course? Course { get; private set; }

    private UserQuizResult() { }

    public static UserQuizResult Create(
        Guid userId,
        Guid? lessonId,
        Guid? courseId,
        int attemptNumber,
        decimal score,
        decimal passScore,
        DateTime? startedAt = null)
    {
        return new UserQuizResult
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LessonId = lessonId,
            CourseId = courseId,
            AttemptNumber = attemptNumber,
            Score = score,
            IsPassed = score >= passScore,
            CompletedAt = DateTime.UtcNow,
            StartedAt = startedAt
        };
    }
}
