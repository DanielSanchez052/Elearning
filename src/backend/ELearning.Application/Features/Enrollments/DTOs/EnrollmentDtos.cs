namespace ELearning.Application.Features.Enrollments.DTOs;
public class EnrollmentDto { public Guid Id { get; set; } public Guid CourseId { get; set; } public string CourseTitle { get; set; } = ""; public DateTime EnrolledAt { get; set; } public DateTime? CompletedAt { get; set; } public int ProgressPercentage { get; set; } }
public class LessonProgressDto { public Guid LessonId { get; set; } public bool IsCompleted { get; set; } public decimal? QuizScore { get; set; } }
