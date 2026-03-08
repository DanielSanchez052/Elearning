namespace ELearning.Application.Features.Quizzes.DTOs;

public sealed record QuizAttemptDto(
    int AttemptNumber,
    decimal Score,
    bool IsPassed,
    DateTime CompletedAt
);
