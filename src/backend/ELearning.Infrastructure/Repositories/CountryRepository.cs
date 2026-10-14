using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly ApplicationDbContext _db;

    public CountryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(Country country, CancellationToken ct = default)
    {
        await _db.Countries.AddAsync(country, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        // Both sides are lowercased on purpose. Lowercasing only the stored
        // column made the check depend on how the caller spelled the argument:
        // CreateCountryCommand passes an already lowercased code and matched,
        // while DevDataSeeder passes the canonical uppercase "CO" and did not,
        // so the seeder re-inserted an existing country and Postgres rejected it
        // against IX_countries_code, crashing startup.
        return _db.Countries
            .AsNoTracking()
            .AnyAsync(c => c.Code.ToLower() == code.ToLower(), ct);
    }

    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default)
    {
        return (await _db.Countries
            .AsNoTracking()
            .ToListAsync(ct)).AsReadOnly();
    }

    public Task<Country?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _db.Countries
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public Task<Country?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
    {
        return _db.Countries
         .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task UpdateAsync(Country country, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
