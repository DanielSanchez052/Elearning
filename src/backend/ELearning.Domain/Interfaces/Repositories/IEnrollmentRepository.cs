using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Repositories;

public interface IEnrollmentRepository
{
    // ── Enrollments ───────────────────────────────────────────────────────────

    Task<CourseEnrollment?> GetByIdAsync(Guid enrollmentId, CancellationToken ct = default);

    Task<CourseEnrollment?> GetByUserAndCourseAsync(Guid userId, Guid courseId, CancellationToken ct = default);

    /// <summary>
    /// Returns all active/completed enrollments for a user, including
    /// course data and lesson progress counts.
    /// </summary>
    Task<IReadOnlyList<CourseEnrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid userId, Guid courseId, CancellationToken ct = default);

    Task AddAsync(CourseEnrollment enrollment, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);

    // ── Lesson Progress ───────────────────────────────────────────────────────

    Task<UserLessonProgress?> GetProgressAsync(Guid enrollmentId, Guid lessonId, CancellationToken ct = default);

    Task AddProgressAsync(UserLessonProgress progress, CancellationToken ct = default);
}
