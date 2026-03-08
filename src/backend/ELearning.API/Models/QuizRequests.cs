namespace ELearning.API.Models;

public sealed record CreateQuizQuestionRequest(
    Guid? LessonId,
    Guid? CourseId,
    int Type,
    string QuestionText,
    decimal PassScore,
    int MaxAttempts,
    bool IsRequired
);

public sealed record UpdateQuizQuestionRequest(
    string QuestionText,
    decimal PassScore,
    int MaxAttempts,
    bool IsRequired
);

public sealed record CreateQuizOptionRequest(
    string OptionText,
    bool IsCorrect,
    int OrderIndex
);

public sealed record UpdateQuizOptionRequest(
    string OptionText,
    bool IsCorrect
);

public sealed record SubmitQuizRequest(
    Guid? LessonId,
    Guid? CourseId,
    IReadOnlyList<Guid> SelectedOptionIds
);
