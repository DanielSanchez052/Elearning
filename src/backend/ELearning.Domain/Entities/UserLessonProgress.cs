namespace ELearning.Domain.Entities;

public class UserLessonProgress
{
    public Guid Id { get; private set; }
    public Guid EnrollmentId { get; private set; }
    public Guid LessonId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public decimal? QuizScore { get; private set; }
    public int AttemptsUsed { get; private set; }

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
            AttemptsUsed = 0
        };
    }

    public void MarkAsCompleted(decimal? quizScore = null)
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        QuizScore = quizScore;
    }

    public void RecordAttempt(decimal score)
    {
        AttemptsUsed++;
        QuizScore = score;
    }

    public bool CanAttempt(int maxAttempts) => AttemptsUsed < maxAttempts;
}
