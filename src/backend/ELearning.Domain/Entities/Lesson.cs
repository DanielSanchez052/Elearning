using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class Lesson
{
    public Guid Id { get; private set; }
    public Guid CourseId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public LessonType Type { get; private set; }
    public string? ContentUrl { get; private set; }
    public int OrderIndex { get; private set; }
    public bool IsRequired { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Course Course { get; private set; } = null!;
    public ICollection<QuizQuestion> QuizQuestions { get; private set; } = new List<QuizQuestion>();
    public ICollection<UserLessonProgress> UserProgress { get; private set; } = new List<UserLessonProgress>();

    private Lesson() { }

    public static Lesson Create(Guid courseId, string title, LessonType type, string? contentUrl, int orderIndex, bool isRequired = true)
    {
        return new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Type = type,
            ContentUrl = contentUrl,
            OrderIndex = orderIndex,
            IsRequired = isRequired,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? contentUrl, bool isRequired)
    {
        Title = title;
        ContentUrl = contentUrl;
        IsRequired = isRequired;
    }

    public void UpdateOrder(int newOrderIndex)
    {
        OrderIndex = newOrderIndex;
    }
}
