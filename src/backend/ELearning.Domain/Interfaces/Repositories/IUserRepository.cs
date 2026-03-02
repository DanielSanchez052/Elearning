using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByEmailTrackedAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByEmailVerifyTokenTrackedAsync(string token, CancellationToken ct = default);
    Task<User?> GetByResetTokenTrackedAsync(string token, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
    int? countryId,
    string? role,
    string? search,
    bool? isEmailVerified,
    int page,
    int pageSize,
    CancellationToken ct = default);

    Task CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}
