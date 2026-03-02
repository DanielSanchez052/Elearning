using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Repositories;

public interface ICountryRepository
{
    Task<Country?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Country?> GetByIdTrackedAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task CreateAsync(Country country, CancellationToken ct = default);
    Task UpdateAsync(Country country, CancellationToken ct = default);
}
