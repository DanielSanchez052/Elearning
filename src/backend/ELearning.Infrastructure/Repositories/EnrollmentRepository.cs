using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
     private readonly ApplicationDbContext _db;

    public EnrollmentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── Enrollments ───────────────────────────────────────────────────────────

    public async Task<CourseEnrollment?> GetByIdAsync(Guid enrollmentId, CancellationToken ct = default)
    {
        return await _db.CourseEnrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Include(e => e.LessonProgress)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);
    }

    public async Task<CourseEnrollment?> GetByUserAndCourseAsync(
        Guid userId, Guid courseId, CancellationToken ct = default)
    {
        return await _db.CourseEnrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Include(e => e.LessonProgress)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId, ct);
    }

    public async Task<IReadOnlyList<CourseEnrollment>> GetByUserAsync(
        Guid userId, CancellationToken ct = default)
    {
        return await _db.CourseEnrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Include(e => e.LessonProgress)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid courseId, CancellationToken ct = default)
    {
        return await _db.CourseEnrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, ct);
    }

    public async Task AddAsync(CourseEnrollment enrollment, CancellationToken ct = default)
    {
        await _db.CourseEnrollments.AddAsync(enrollment, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    // ── Lesson Progress ───────────────────────────────────────────────────────

    public async Task<UserLessonProgress?> GetProgressAsync(
        Guid enrollmentId, Guid lessonId, CancellationToken ct = default)
    {
        return await _db.UserLessonProgress
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId && p.LessonId == lessonId, ct);
    }

    public async Task AddProgressAsync(UserLessonProgress progress, CancellationToken ct = default)
    {
        await _db.UserLessonProgress.AddAsync(progress, ct);
    }
}
