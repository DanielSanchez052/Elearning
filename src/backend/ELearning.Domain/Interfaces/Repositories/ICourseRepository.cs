using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Repositories;

public interface ICourseRepository
{
    // ── Lectura ───────────────────────────────────────────────────────────────

    Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Course?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Catálogo paginado filtrado por país del usuario.
    /// Incluye cursos globales (IsGlobal = true) y cursos asignados al país.
    /// Solo devuelve cursos activos (IsActive = true).
    /// </summary>
    Task<(IReadOnlyList<Course> Courses, int TotalCount)> GetCatalogAsync(
        int countryId,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Lista de cursos para el panel admin — sin filtro de país ni estado activo.
    /// </summary>
    Task<(IReadOnlyList<Course> Courses, int TotalCount)> GetAdminListAsync(
        Guid? instructorId, // null = todos los instructores
        int? countryId,    // null = todos los países
        bool? isActive,     // null = todos los estados
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<bool> ExistsByTitleAndInstructorAsync(
        string title,
        Guid instructorId,
        CancellationToken ct = default);

    // ── Escritura ─────────────────────────────────────────────────────────────

    Task CreateAsync(Course course, CancellationToken ct = default);
    Task UpdateAsync(Course course, CancellationToken ct = default);

    // ── Países asignados ──────────────────────────────────────────────────────

    Task<IReadOnlyList<CourseCountry>> GetCourseCountriesAsync(
        Guid courseId,
        CancellationToken ct = default);

    Task SetCourseCountriesAsync(
        Guid courseId,
        List<int> countryIds,
        CancellationToken ct = default);
}