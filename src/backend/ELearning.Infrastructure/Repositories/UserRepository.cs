using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db) => _db = db;

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();
        return _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email.ToLower() == normalized, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();
        return _db.Users
            .AsNoTracking()
            .Include(u => u.Country)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, ct);
    }

    // Con tracking — para modificar y guardar (RecordLogin, VerifyEmail, etc.)
    public Task<User?> GetByEmailTrackedAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();
        return _db.Users
            .Include(u => u.Country)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, ct);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Users
            .AsNoTracking()
            .Include(u => u.Country)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    // Con tracking — para handlers que modifican el usuario
    public Task<User?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Users
            .Include(u => u.Country)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
    public Task<User?> GetByEmailVerifyTokenTrackedAsync(string token, CancellationToken ct = default)
    {
        return _db.Users
            .FirstOrDefaultAsync(u => u.EmailVerifyToken == token, ct);
    }

    public Task<User?> GetByResetTokenTrackedAsync(string token, CancellationToken ct = default)
    {
        return _db.Users
            .FirstOrDefaultAsync(u => u.ResetToken == token, ct);
    }

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int? countryId, string? role, string? search, bool? isEmailVerified, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Users.AsNoTracking().Include(u => u.Country).AsQueryable();

        if (countryId.HasValue)
        {
            query = query.Where(u => u.CountryId == countryId);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role.ToString() == role);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(searchLower) ||
                                     u.FullName.ToLower().Contains(searchLower));
        }

        if (isEmailVerified.HasValue)
        {
            query = query.Where(u => u.IsEmailVerified == isEmailVerified);
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (users.AsReadOnly(), totalCount);
    }
}