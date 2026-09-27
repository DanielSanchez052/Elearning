namespace ELearning.Application.Features.Quizzes.DTOs;

public sealed record QuizResultDto(
    decimal Score,
    bool IsPassed,
    decimal PassScore,
    int TotalQuestions,
    int CorrectAnswers,
    int AttemptNumber,
    int MaxAttempts,
    string Feedback,
    DateTime CompletedAt,
    // True only when this submission moved the enrollment to Completed (passed final exam).
    bool CourseCompleted = false
);