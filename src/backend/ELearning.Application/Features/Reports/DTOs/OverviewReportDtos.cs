namespace ELearning.Application.Features.Reports.DTOs;

/// <summary>Métricas principales de un ámbito (la plataforma o un país) en el periodo.</summary>
/// <param name="CompletionRate">De las inscripciones del periodo, % que ya están completadas.</param>
/// <param name="AvgDaysToComplete">Días promedio entre inscripción y finalización de los cursos completados en el periodo.</param>
public sealed record ReportKpisDto(
    int Students,
    int ActiveStudents,
    int Enrollments,
    int Completions,
    double? CompletionRate,
    double? AvgDaysToComplete);

public sealed record PopularCourseDto(
    Guid CourseId,
    string Title,
    int Enrollments,
    int EnrollmentsCompleted,
    double? CompletionRate);

public sealed record CountryReportRowDto(
    int CountryId,
    string CountryName,
    ReportKpisDto Kpis);

/// <param name="Period">Código del periodo: 7d, 30d, 90d o all.</param>
/// <param name="Since">Inicio del periodo (UTC); null para todo el histórico.</param>
/// <param name="CountryId">País del reporte; null = todos los países.</param>
/// <param name="Countries">Comparativa por país; solo para el superadmin cuando no filtra por país.</param>
public sealed record OverviewReportDto(
    string Period,
    DateTime? Since,
    int? CountryId,
    string? CountryName,
    ReportKpisDto Kpis,
    IReadOnlyList<PopularCourseDto> PopularCourses,
    IReadOnlyList<CountryReportRowDto>? Countries);
