namespace ELearning.Domain.ReadModels.Reports;

/// <summary>
/// Filtro común de los reportes. <see cref="Since"/> es el inicio del periodo (null = todo el
/// histórico) y <see cref="CountryId"/> limita a los estudiantes de un país (null = todos).
/// Solo cuentan usuarios con rol estudiante.
/// </summary>
public sealed record ReportFilter(DateTime? Since, int? CountryId);

/// <summary>
/// Conteos de un ámbito (la plataforma o un país):
/// <list type="bullet">
///   <item><see cref="Students"/>: estudiantes registrados (sin límite de periodo).</item>
///   <item><see cref="ActiveStudents"/>: estudiantes que iniciaron sesión dentro del periodo.</item>
///   <item><see cref="Enrollments"/>: inscripciones hechas dentro del periodo, y
///   <see cref="EnrollmentsCompleted"/>, cuántas de ellas ya están completadas.</item>
///   <item><see cref="Completions"/>: cursos completados dentro del periodo, y
///   <see cref="AvgDaysToComplete"/>, sus días promedio desde la inscripción.</item>
/// </list>
/// </summary>
public sealed record ReportCounts(
    int Students,
    int ActiveStudents,
    int Enrollments,
    int EnrollmentsCompleted,
    int Completions,
    double? AvgDaysToComplete);

/// <summary>Inscripciones del periodo a un curso y cuántas de ellas ya están completadas.</summary>
public sealed record CourseEnrollmentCounts(
    Guid CourseId,
    string Title,
    int Enrollments,
    int EnrollmentsCompleted);

/// <summary>Los mismos conteos que <see cref="ReportCounts"/>, para un país.</summary>
public sealed record CountryReportCounts(int CountryId, string CountryName, ReportCounts Counts);
