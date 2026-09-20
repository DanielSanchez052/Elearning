using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Entities;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _db;

    public NotificationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
