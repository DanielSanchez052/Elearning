namespace ELearning.Application.Features.Quizzes.DTOs;
public class QuizQuestionDto { public Guid Id { get; set; } public string QuestionText { get; set; } = ""; public decimal PassScore { get; set; } public int MaxAttempts { get; set; } public List<QuizOptionDto> Options { get; set; } = new(); }
public class QuizOptionDto { public Guid Id { get; set; } public string OptionText { get; set; } = ""; }
public class SubmitQuizDto { public Guid LessonId { get; set; } public List<Guid> SelectedOptionIds { get; set; } = new(); }
