using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Enrollments.Queries;

public record GetCourseProgressQuery(Guid UserId, Guid CourseId) : IQuery<CourseProgressDto>;

public class GetCourseProgressHandler : IQueryHandler<GetCourseProgressQuery, CourseProgressDto>
{
    private readonly IEnrollmentRepository _enrollments;

    public GetCourseProgressHandler(IEnrollmentRepository enrollments)
    {
        _enrollments = enrollments;
    }

    public async Task<Result<CourseProgressDto>> HandleAsync(GetCourseProgressQuery query, CancellationToken ct = default)
    {
        var enrollment = await _enrollments.GetByUserAndCourseAsync(query.UserId, query.CourseId, ct);

        if (enrollment == null)
            return Result.NotFound<CourseProgressDto>("No estas inscrito en este curso.");

        var course = enrollment.Course;
        var progressMap = enrollment.LessonProgress.ToDictionary(p => p.LessonId);

        var lessonDtos = course.Lessons
            .OrderBy(l => l.OrderIndex)
            .Select(l =>
            {
                progressMap.TryGetValue(l.Id, out var prog);
                return new LessonProgressDto(
                    LessonId: l.Id,
                    Title: l.Title,
                    Type: l.Type.ToString().ToLowerInvariant(),
                    OrderIndex: l.OrderIndex,
                    IsRequired: l.IsRequired,
                    IsCompleted: prog?.IsCompleted ?? false,
                    CompletedAt: prog?.CompletedAt,
                    LastAccessedAt: prog?.LastAccessedAt
                );
            })
            .ToList();

        var required = lessonDtos.Where(l => l.IsRequired).ToList();
        int completedRequired = required.Count(l => l.IsCompleted);
        int progressPercent = required.Count > 0
            ? (int)Math.Round(completedRequired * 100.0 / required.Count)
            : 100;

        return new CourseProgressDto(
            EnrollmentId: enrollment.Id,
            CourseId: course.Id,
            CourseTitle: course.Title,
            CourseThumbnailUrl: course.ThumbnailUrl,
            Status: enrollment.Status,
            ProgressPercent: progressPercent,
            CompletedLessons: completedRequired,
            RequiredLessons: required.Count,
            EnrolledAt: enrollment.EnrolledAt,
            CompletedAt: enrollment.CompletedAt,
            Lessons: lessonDtos
        );
    }
}
