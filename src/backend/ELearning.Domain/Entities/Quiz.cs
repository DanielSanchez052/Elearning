using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class QuizQuestion
{
    public Guid Id { get; private set; }
    public QuizType Type { get; private set; } = QuizType.PerLesson;

    // FKs condicionales
    public Guid? LessonId { get; private set; }
    public Guid? CourseId { get; private set; }

    public string QuestionText { get; private set; } = string.Empty;
    public decimal PassScore { get; private set; }
    public int MaxAttempts { get; private set; }
    public int OrderIndex { get; private set; }
    public bool IsRequired { get; private set; } = true;

    public Lesson? Lesson { get; private set; }
    public Course? Course { get; private set; }
    public ICollection<QuizOption> Options { get; private set; } = new List<QuizOption>();

    private QuizQuestion() { }

    // Factory para pregunta de lección
    public static QuizQuestion CreatePerLesson(
        Guid lessonId,
        string questionText,
        decimal passScore = 60.00m,
        int maxAttempts = 3,
        int orderIndex = 1,
        bool isRequired = true)
    {
        return new QuizQuestion
        {
            Id = Guid.NewGuid(),
            Type = QuizType.PerLesson,
            LessonId = lessonId,
            QuestionText = questionText,
            PassScore = passScore,
            MaxAttempts = maxAttempts,
            OrderIndex = orderIndex,
            IsRequired = isRequired
        };
    }

    // Factory para examen de curso
    public static QuizQuestion CreateCourseExam(
        Guid courseId,
        string questionText,
        decimal passScore = 70.00m,
        int maxAttempts = 3,
        int orderIndex = 1,
        bool isRequired = true)
    {
        return new QuizQuestion
        {
            Id = Guid.NewGuid(),
            Type = QuizType.CourseExam,
            CourseId = courseId,
            QuestionText = questionText,
            PassScore = passScore,
            MaxAttempts = maxAttempts,
            OrderIndex = orderIndex,
            IsRequired = isRequired
        };
    }

    public void UpdateQuestion(string questionText, decimal passScore, int maxAttempts, bool isRequired)
    {
        QuestionText = questionText;
        PassScore = passScore;
        MaxAttempts = maxAttempts;
        IsRequired = isRequired;
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

    public static QuizOption Create(Guid questionId, string optionText, bool isCorrect, int orderIndex)
    {
        return new QuizOption
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            OptionText = optionText,
            IsCorrect = isCorrect,
            OrderIndex = orderIndex
        };
    }

    public void Update(string optionText, bool isCorrect)
    {
        OptionText = optionText;
        IsCorrect = isCorrect;
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
