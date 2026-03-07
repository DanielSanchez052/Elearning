using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Enrollments.Queries;

public record GetMyEnrollmentsQuery(Guid UserId) : IQuery<IReadOnlyList<EnrollmentSummaryDto>>;

public class GetMyEnrollmentsHandler
    : IQueryHandler<GetMyEnrollmentsQuery, IReadOnlyList<EnrollmentSummaryDto>>
{
    private readonly IEnrollmentRepository _enrollments;

    public GetMyEnrollmentsHandler(IEnrollmentRepository enrollments)
    {
        _enrollments = enrollments;
    }

    public async Task<Result<IReadOnlyList<EnrollmentSummaryDto>>> HandleAsync(GetMyEnrollmentsQuery query, CancellationToken ct = default)
    {
        var enrollments = await _enrollments.GetByUserAsync(query.UserId, ct);

        return enrollments.Select(e =>
        {
            var lessons = e.Course.Lessons.ToList();
            var requiredLessons = lessons.Where(l => l.IsRequired).ToList();
            var completedIds = e.LessonProgress
                .Where(p => p.IsCompleted)
                .Select(p => p.LessonId)
                .ToHashSet();

            int completedRequired = requiredLessons.Count(l => completedIds.Contains(l.Id));
            int progressPercent = requiredLessons.Count > 0
                ? (int)Math.Round(completedRequired * 100.0 / requiredLessons.Count)
                : 100;

            return new EnrollmentSummaryDto(
                EnrollmentId: e.Id,
                CourseId: e.CourseId,
                CourseTitle: e.Course.Title,
                CourseThumbnailUrl: e.Course.ThumbnailUrl,
                Status: e.Status,
                TotalLessons: lessons.Count,
                RequiredLessons: requiredLessons.Count,
                CompletedLessons: completedRequired,
                ProgressPercent: progressPercent,
                EnrolledAt: e.EnrolledAt,
                CompletedAt: e.CompletedAt,
                DeadlineAt: e.DeadlineAt
            );
        }).ToList();
    }
}
