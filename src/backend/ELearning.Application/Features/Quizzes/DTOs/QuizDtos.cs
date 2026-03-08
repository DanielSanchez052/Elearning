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

public sealed record QuizOptionDto(
    Guid Id,
    string OptionText,
    int OrderIndex
);

public sealed record QuizResultDto(
    decimal Score,
    bool IsPassed,
    decimal PassScore,
    int TotalQuestions,
    int CorrectAnswers,
    int AttemptNumber,
    int MaxAttempts,
    string Feedback,
    DateTime CompletedAt
);

public sealed record QuizAttemptDto(
    int AttemptNumber,
    decimal Score,
    bool IsPassed,
    DateTime CompletedAt
);

public sealed record SubmitQuizRequestDto(
    Guid? LessonId,
    Guid? CourseId,
    IReadOnlyList<Guid> SelectedOptionIds
);
