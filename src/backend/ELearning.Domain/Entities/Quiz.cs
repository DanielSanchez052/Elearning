namespace ELearning.Domain.Entities;

public class QuizQuestion
{
    public Guid Id { get; private set; }
    public Guid LessonId { get; private set; }
    public string QuestionText { get; private set; } = string.Empty;
    public decimal PassScore { get; private set; }
    public int MaxAttempts { get; private set; }
    public int OrderIndex { get; private set; }

    public Lesson Lesson { get; private set; } = null!;
    public ICollection<QuizOption> Options { get; private set; } = new List<QuizOption>();

    private QuizQuestion() { }

    public static QuizQuestion Create(Guid lessonId, string questionText, decimal passScore = 60.00m, int maxAttempts = 3, int orderIndex = 1)
    {
        return new QuizQuestion
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            QuestionText = questionText,
            PassScore = passScore,
            MaxAttempts = maxAttempts,
            OrderIndex = orderIndex
        };
    }

    public void UpdateQuestion(string questionText, decimal passScore, int maxAttempts)
    {
        QuestionText = questionText;
        PassScore = passScore;
        MaxAttempts = maxAttempts;
    }

    public void AddOption(QuizOption option)
    {
        Options.Add(option);
    }
}

public class QuizOption
{
    public Guid Id { get; private set; }
    public Guid QuestionId { get; private set; }
    public string OptionText { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int OrderIndex { get; private set; }

    public QuizQuestion Question { get; private set; } = null!;

    private QuizOption() { }

    public static QuizOption Create(string optionText, bool isCorrect, int orderIndex)
    {
        return new QuizOption
        {
            Id = Guid.NewGuid(),
            OptionText = optionText,
            IsCorrect = isCorrect,
            OrderIndex = orderIndex
        };
    }

    public void MarkAsCorrect()
    {
        IsCorrect = true;
    }

    public void MarkAsIncorrect()
    {
        IsCorrect = false;
    }
}
