namespace ELearning.Application.Features.Quizzes.DTOs;

/// <summary>
/// Returned when a student starts (or resumes) a course final-exam attempt.
/// StartedAt is the server-side start of the attempt; resuming returns the original value.
/// </summary>
public sealed record StartCourseExamResultDto(
    Guid SessionId,
    int AttemptNumber,
    DateTime StartedAt,
    IReadOnlyList<QuizQuestionDto> Questions
);
