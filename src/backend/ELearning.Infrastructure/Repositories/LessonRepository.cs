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

    public async Task UpdateOrdersAsync(IEnumerable<(Guid LessonId, int NewOrder)> orders, CancellationToken ct = default)
    {
        var orderList = orders.ToList();
        if (orderList.Count == 0)
            return;

        // Usar el execution strategy de la base de datos para manejar reintentos
        // esto es necesario con PostgreSQL que usa NpgsqlRetryingExecutionStrategy
        var strategy = _db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // Paso 1: Asignar valores temporales negativos y distintos para evitar conflictos
                // Usamos -1000, -1001, -1002... para que no colisionen con valores finales positivos
                var tempValue = -1000;
                foreach (var (lessonId, _) in orderList)
                {
                    await _db.Lessons
                        .Where(l => l.Id == lessonId)
                        .ExecuteUpdateAsync(
                            s => s.SetProperty(l => l.OrderIndex, tempValue),
                            ct);
                    tempValue--;
                }

                // Paso 2: Asignar los valores finales del nuevo orden
                foreach (var (lessonId, newOrder) in orderList)
                {
                    await _db.Lessons
                        .Where(l => l.Id == lessonId)
                        .ExecuteUpdateAsync(
                            s => s.SetProperty(l => l.OrderIndex, newOrder),
                            ct);
                }

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}