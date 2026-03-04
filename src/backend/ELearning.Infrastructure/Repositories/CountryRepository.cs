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
        return _db.Countries
            .AsNoTracking()
            .AnyAsync(c => c.Code.ToLower() == code, ct);
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
