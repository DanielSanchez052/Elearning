namespace ELearning.Application.Features.Quizzes.DTOs;

/// <summary>
/// Returned when a student starts (or resumes) a course final-exam attempt.
/// StartedAt is the server-side start of the attempt; resuming returns the original value.
/// ServerNow lets the client compute elapsed time (ServerNow - StartedAt) on the server's
/// clock, so a skewed client clock cannot shrink or extend the displayed countdown.
/// </summary>
public sealed record StartCourseExamResultDto(
    Guid SessionId,
    int AttemptNumber,
    DateTime StartedAt,
    DateTime ServerNow,
    IReadOnlyList<QuizQuestionDto> Questions
);
