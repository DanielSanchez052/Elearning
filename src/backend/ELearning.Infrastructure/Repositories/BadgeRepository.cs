using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Infrastructure.Repositories;

public class BadgeRepository : IBadgeRepository
{
    // NOTE: real implementation lands in Track B / task B2 (Infrastructure).
    // These stubs only exist so the solution keeps compiling after the B1
    // domain-only interface extension.
    public Task<Badge?> GetByCodeAsync(BadgeCode code, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Badge>> GetAllAsync(CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<UserBadge>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<bool> HasBadgeAsync(Guid userId, int badgeId, Guid? courseId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<bool> TryAddAsync(UserBadge badge, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        throw new NotImplementedException();
}
