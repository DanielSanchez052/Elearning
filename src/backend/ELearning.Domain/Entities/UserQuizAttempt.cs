namespace ELearning.Domain.Entities;

public class UserQuizAttempt
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid SelectedOptionId { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTime AttemptedAt { get; private set; }

    public User User { get; private set; } = null!;
    public QuizQuestion Question { get; private set; } = null!;
    public QuizOption SelectedOption { get; private set; } = null!;

    private UserQuizAttempt() { }

    public static UserQuizAttempt Create(
        Guid userId,
        Guid questionId,
        Guid selectedOptionId,
        int attemptNumber)
    {
        return new UserQuizAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            QuestionId = questionId,
            SelectedOptionId = selectedOptionId,
            AttemptNumber = attemptNumber,
            AttemptedAt = DateTime.UtcNow
        };
    }
}
