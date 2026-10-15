using ELearning.Domain.ReadModels.Reports;

namespace ELearning.Domain.Interfaces.Repositories;

/// <summary>
/// Consultas agregadas para los reportes: devuelve conteos, no entidades. Las reglas que se
/// calculan sobre ellos (tasas, rangos, posiciones) viven en Application.
/// </summary>
public interface IReportRepository
{
    Task<ReportCounts> GetCountsAsync(ReportFilter filter, CancellationToken ct = default);

    /// <summary>Cursos con más inscripciones dentro del periodo, de mayor a menor.</summary>
    Task<IReadOnlyList<CourseEnrollmentCounts>> GetTopCoursesAsync(
        ReportFilter filter, int top, CancellationToken ct = default);

    /// <summary>Una fila por país activo, ordenadas por nombre. Ignora <see cref="ReportFilter.CountryId"/>.</summary>
    Task<IReadOnlyList<CountryReportCounts>> GetCountsByCountryAsync(
        ReportFilter filter, CancellationToken ct = default);
}
