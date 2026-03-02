namespace ELearning.Application.Features.Reports.DTOs;
public class DashboardDto { public int TotalUsers { get; set; } public int TotalCourses { get; set; } public int TotalEnrollments { get; set; } public int CoursesCompleted { get; set; } }
public class LeaderboardDto { public Guid UserId { get; set; } public string FullName { get; set; } = ""; public int CompletedCourses { get; set; } public int Rank { get; set; } }
