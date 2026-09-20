namespace ELearning.Application.Features.Quizzes.DTOs;

// Admin-only DTOs — include IsCorrect for content-authoring staff.
// Do NOT reuse these for student-facing endpoints (QuizQuestionDto/QuizOptionDto
// intentionally omit IsCorrect to prevent answer leaks).

public sealed record QuizOptionAdminDto(
    Guid Id,
    string OptionText,
    bool IsCorrect,
    int OrderIndex
);

public sealed record QuizQuestionAdminDto(
    Guid Id,
    string QuestionText,
    int Type,
    bool IsRequired,
    decimal PassScore,
    int MaxAttempts,
    int OrderIndex,
    IReadOnlyList<QuizOptionAdminDto> Options,
    Guid? LessonId,
    Guid? CourseId
);
