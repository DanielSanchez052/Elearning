using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class LessonRepository : ILessonRepository
{
    private readonly ApplicationDbContext _db;

    public LessonRepository(ApplicationDbContext db) => _db = db;

    public Task<Lesson?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public Task<Lesson?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Lessons
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<IReadOnlyList<Lesson>> GetByCourseAsync(
        Guid courseId,
        CancellationToken ct = default)
    {
        return await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync(ct);
    }

    public async Task<int> GetMaxOrderIndexAsync(
        Guid courseId,
        CancellationToken ct = default)
    {
        // Si no hay lecciones devuelve 0 — la primera lección quedará en OrderIndex 1
        return await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .MaxAsync(l => (int?)l.OrderIndex, ct) ?? 0;
    }

    public async Task CreateAsync(Lesson lesson, CancellationToken ct = default)
    {
        await _db.Lessons.AddAsync(lesson, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Lesson lesson, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Lesson lesson, CancellationToken ct = default)
    {
        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateOrdersAsync(
        IEnumerable<(Guid LessonId, int NewOrder)> orders,
        CancellationToken ct = default)
    {
        // Cargar todas las lecciones a actualizar en una sola query
        var ids = orders.Select(o => o.LessonId).ToList();

        var lessons = await _db.Lessons
            .Where(l => ids.Contains(l.Id))
            .ToListAsync(ct);

        var orderMap = orders.ToDictionary(o => o.LessonId, o => o.NewOrder);

        foreach (var lesson in lessons)
            lesson.UpdateOrder(orderMap[lesson.Id]);

        await _db.SaveChangesAsync(ct);
    }
}