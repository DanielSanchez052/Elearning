namespace ELearning.Application.Features.Quizzes.DTOs;

public sealed record QuizQuestionDto(
    Guid Id,
    string QuestionText,
    int Type,
    bool IsRequired,
    decimal PassScore,
    int MaxAttempts,
    int OrderIndex,
    IReadOnlyList<QuizOptionDto> Options,
    Guid? LessonId,
    Guid? CourseId
);
