using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.ReadModels.Reports;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _db;

    public ReportRepository(ApplicationDbContext db) => _db = db;

    public async Task<ReportCounts> GetCountsAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var since = filter.Since;
        var students = Students(filter.CountryId);
        var enrollments = StudentEnrollments(filter.CountryId);

        var enrolledInPeriod = enrollments.Where(e => since == null || e.EnrolledAt >= since);
        var completedInPeriod = enrollments.Where(e =>
            e.Status == EnrollmentStatus.Completed
            && e.CompletedAt != null
            && (since == null || e.CompletedAt >= since));

        return new ReportCounts(
            Students: await students.CountAsync(ct),
            ActiveStudents: await students.CountAsync(
                u => u.LastLoginAt != null && (since == null || u.LastLoginAt >= since), ct),
            Enrollments: await enrolledInPeriod.CountAsync(ct),
            EnrollmentsCompleted: await enrolledInPeriod.CountAsync(e => e.Status == EnrollmentStatus.Completed, ct),
            Completions: await completedInPeriod.CountAsync(ct),
            AvgDaysToComplete: await completedInPeriod
                .Select(e => (double?)(e.CompletedAt!.Value - e.EnrolledAt).TotalDays)
                .AverageAsync(ct));
    }

    public async Task<IReadOnlyList<CourseEnrollmentCounts>> GetTopCoursesAsync(
        ReportFilter filter, int top, CancellationToken ct = default)
    {
        var since = filter.Since;

        return await StudentEnrollments(filter.CountryId)
            .Where(e => since == null || e.EnrolledAt >= since)
            .GroupBy(e => new { e.CourseId, e.Course.Title })
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key.Title)
            .Take(top)
            .Select(g => new CourseEnrollmentCounts(
                g.Key.CourseId,
                g.Key.Title,
                g.Count(),
                g.Count(e => e.Status == EnrollmentStatus.Completed)))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CountryReportCounts>> GetCountsByCountryAsync(
        ReportFilter filter, CancellationToken ct = default)
    {
        var countries = await _db.Countries
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);

        // Pocos países (el alcance limita el lanzamiento a 3): una ronda de conteos por país
        // reutiliza exactamente las mismas definiciones que el resumen general.
        var rows = new List<CountryReportCounts>(countries.Count);
        foreach (var country in countries)
        {
            var counts = await GetCountsAsync(filter with { CountryId = country.Id }, ct);
            rows.Add(new CountryReportCounts(country.Id, country.Name, counts));
        }

        return rows;
    }

    // Solo cuentan estudiantes: el staff también puede inscribirse, pero no es el público de los reportes.
    private IQueryable<User> Students(int? countryId) =>
        _db.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Student && (countryId == null || u.CountryId == countryId));

    private IQueryable<CourseEnrollment> StudentEnrollments(int? countryId) =>
        _db.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.User.Role == UserRole.Student && (countryId == null || e.User.CountryId == countryId));
}
