namespace ELearning.Application.Features.Quizzes.DTOs;

public sealed record SubmitQuizRequestDto(
    Guid? LessonId,
    Guid? CourseId,
    IReadOnlyList<Guid> SelectedOptionIds
);
