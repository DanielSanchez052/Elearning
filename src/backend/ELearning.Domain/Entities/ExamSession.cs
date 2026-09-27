namespace ELearning.Domain.Entities;

/// <summary>
/// One explicit start of a course final-exam attempt. Records when the student started
/// the attempt so the submission can be timed server-side. At most one session per
/// user+course may be open (not yet submitted) at a time.
/// </summary>
public class ExamSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    private ExamSession() { }

    public static ExamSession Start(Guid userId, Guid courseId, int attemptNumber)
    {
        return new ExamSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            AttemptNumber = attemptNumber,
            StartedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Closes the session. Idempotent: a second call keeps the original SubmittedAt.
    /// </summary>
    public void MarkSubmitted()
    {
        if (SubmittedAt is not null)
            return;

        SubmittedAt = DateTime.UtcNow;
    }

    public bool IsOpen => SubmittedAt is null;
}
