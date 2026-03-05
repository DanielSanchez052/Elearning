using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _db;

    public CourseRepository(ApplicationDbContext db) => _db = db;

    // ── Lectura ───────────────────────────────────────────────────────────────

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Courses
            .AsNoTracking()
            .Include(c => c.CreatedByUser)
            .Include(c => c.CourseCountries)
                .ThenInclude(cc => cc.Country)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<Course?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Courses
            .Include(c => c.CreatedByUser)
            .Include(c => c.CourseCountries)
                .ThenInclude(cc => cc.Country)
            .Include(c => c.Enrollments) // necesario para verificar inscritos al eliminar
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Course> Courses, int TotalCount)> GetCatalogAsync(int countryId, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        // Cursos que el usuario puede ver:
        // 1. Cursos globales (IsGlobal = true)
        // 2. Cursos asignados al país del usuario
        // Solo activos
        var query = _db.Courses
            .AsNoTracking()
            .Include(c => c.CreatedByUser)
            .Where(c => c.IsActive)
            .Where(c => c.IsGlobal ||
                        c.CourseCountries.Any(cc => cc.CountryId == countryId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(c => c.Title.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (courses.AsReadOnly(), totalCount);
    }

    public async Task<(IReadOnlyList<Course> Courses, int TotalCount)> GetAdminListAsync(
        Guid? instructorId,
        int? countryId,
        bool? isActive,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Courses
            .AsNoTracking()
            .Include(c => c.CreatedByUser)
            .AsQueryable();

        if (instructorId.HasValue)
            query = query.Where(c => c.CreatedBy == instructorId.Value);

        if (countryId.HasValue)
            query = query.Where(c => c.IsGlobal ||
                                     c.CourseCountries.Any(cc => cc.CountryId == countryId.Value));

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(c => c.Title.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (courses.AsReadOnly(), totalCount);
    }

    public Task<bool> ExistsByTitleAndInstructorAsync(
        string title,
        Guid instructorId,
        CancellationToken ct = default)
    {
        return _db.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Title.ToLower() == title.ToLowerInvariant()
                        && c.CreatedBy == instructorId, ct);
    }

    // ── Escritura ─────────────────────────────────────────────────────────────

    public async Task CreateAsync(Course course, CancellationToken ct = default)
    {
        await _db.Courses.AddAsync(course, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Course course, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    // ── Países asignados ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CourseCountry>> GetCourseCountriesAsync(
        Guid courseId,
        CancellationToken ct = default)
    {
        return await _db.Set<CourseCountry>()
            .AsNoTracking()
            .Include(cc => cc.Country)
            .Where(cc => cc.CourseId == courseId)
            .ToListAsync(ct);
    }

    public async Task SetCourseCountriesAsync(
        Guid courseId,
        List<int> countryIds,
        CancellationToken ct = default)
    {
        // Eliminar asignaciones actuales y reemplazar con las nuevas
        // Es más simple y seguro que hacer un diff para este volumen de datos
        var existing = await _db.Set<CourseCountry>()
            .Where(cc => cc.CourseId == courseId)
            .ToListAsync(ct);

        _db.Set<CourseCountry>().RemoveRange(existing);

        var newAssignments = countryIds
            .Distinct()
            .Select(cid => CourseCountry.Create(courseId, cid));

        await _db.Set<CourseCountry>().AddRangeAsync(newAssignments, ct);
        await _db.SaveChangesAsync(ct);
    }
}