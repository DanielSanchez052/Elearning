using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Domain.Interfaces.Repositories;

public interface IBadgeRepository
{
    Task<Badge?> GetByCodeAsync(BadgeCode code, CancellationToken ct = default);

    Task<IReadOnlyList<Badge>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<UserBadge>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    Task<bool> HasBadgeAsync(Guid userId, int badgeId, Guid? courseId, CancellationToken ct = default);

    /// <summary>
    /// Attempts to insert a new UserBadge, relying on the unique (UserId, BadgeId, CourseId)
    /// index as the race-safety net. Returns false (instead of throwing) when a concurrent
    /// request already awarded the same badge/course combination.
    /// </summary>
    Task<bool> TryAddAsync(UserBadge badge, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
