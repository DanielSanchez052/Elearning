using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ELearning.Infrastructure.Repositories;

public class BadgeRepository : IBadgeRepository
{
    private const string PostgresUniqueViolationSqlState = "23505";

    private readonly ApplicationDbContext _db;

    public BadgeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Badge?> GetByCodeAsync(BadgeCode code, CancellationToken ct = default)
    {
        var codeValue = code.ToString();
        return await _db.Badges.FirstOrDefaultAsync(b => b.Code == codeValue, ct);
    }

    public async Task<IReadOnlyList<Badge>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Badges.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserBadge>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserBadges
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .Include(ub => ub.Course)
            .OrderByDescending(ub => ub.ObtainedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> HasBadgeAsync(Guid userId, int badgeId, Guid? courseId, CancellationToken ct = default)
    {
        return await _db.UserBadges
            .AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId && ub.CourseId == courseId, ct);
    }

    public async Task<bool> TryAddAsync(UserBadge badge, CancellationToken ct = default)
    {
        await _db.UserBadges.AddAsync(badge, ct);

        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx &&
                                            pgEx.SqlState == PostgresUniqueViolationSqlState)
        {
            // Lost the race to a concurrent award of the same (user, badge, course)
            // combination. Detach so the tracked-but-unsaved entity doesn't corrupt
            // a later SaveChangesAsync call in the same request/DbContext.
            _db.Entry(badge).State = EntityState.Detached;
            return false;
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
