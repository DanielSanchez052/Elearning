using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Application.Features.Reports.Common;
using ELearning.Application.Features.Reports.DTOs;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.ReadModels.Reports;

namespace ELearning.Application.Features.Reports.Queries;

/// <summary>
/// Resumen de la plataforma o de un país: KPIs, cursos más populares y, para el superadmin
/// sin filtro de país, la comparativa por país. Solo admin y superadmin (ver <see cref="ReportAccess"/>).
/// </summary>
public sealed record GetOverviewReportQuery(
    ReportPeriod Period,
    int? CountryId,
    string RequesterRole,
    int RequesterCountryId
) : IQuery<OverviewReportDto>;

public sealed class GetOverviewReportHandler : IQueryHandler<GetOverviewReportQuery, OverviewReportDto>
{
    public const int PopularCoursesCount = 5;

    private readonly IReportRepository _reports;
    private readonly ICountryRepository _countries;
    private readonly TimeProvider _time;

    public GetOverviewReportHandler(IReportRepository reports, ICountryRepository countries, TimeProvider time)
    {
        _reports = reports;
        _countries = countries;
        _time = time;
    }

    public async Task<Result<OverviewReportDto>> HandleAsync(GetOverviewReportQuery query, CancellationToken ct = default)
    {
        if (!ReportAccess.CanViewPlatformReports(query.RequesterRole))
            return Result.Forbidden<OverviewReportDto>(ReportAccess.ForbiddenMessage);

        var countryId = ReportAccess.ResolveCountry(query.RequesterRole, query.RequesterCountryId, query.CountryId);
        var since = query.Period.Since(_time.GetUtcNow().UtcDateTime);
        var filter = new ReportFilter(since, countryId);

        string? countryName = null;
        if (countryId is { } id)
        {
            var country = await _countries.GetByIdAsync(id, ct);
            if (country is null)
                return Result.NotFound<OverviewReportDto>($"País con id '{id}' no encontrado.");
            countryName = country.Name;
        }

        var counts = await _reports.GetCountsAsync(filter, ct);
        var topCourses = await _reports.GetTopCoursesAsync(filter, PopularCoursesCount, ct);

        IReadOnlyList<CountryReportRowDto>? countries = null;
        if (countryId is null)
        {
            var byCountry = await _reports.GetCountsByCountryAsync(filter, ct);
            countries = byCountry
                .Select(c => new CountryReportRowDto(c.CountryId, c.CountryName, ToKpis(c.Counts)))
                .ToList();
        }

        return Result.Success(new OverviewReportDto(
            Period: query.Period.ToCode(),
            Since: since,
            CountryId: countryId,
            CountryName: countryName,
            Kpis: ToKpis(counts),
            PopularCourses: topCourses
                .Select(c => new PopularCourseDto(
                    c.CourseId, c.Title, c.Enrollments, c.EnrollmentsCompleted,
                    ReportMath.Percentage(c.EnrollmentsCompleted, c.Enrollments)))
                .ToList(),
            Countries: countries));
    }

    private static ReportKpisDto ToKpis(ReportCounts c) => new(
        Students: c.Students,
        ActiveStudents: c.ActiveStudents,
        Enrollments: c.Enrollments,
        Completions: c.Completions,
        CompletionRate: ReportMath.Percentage(c.EnrollmentsCompleted, c.Enrollments),
        AvgDaysToComplete: ReportMath.Days(c.AvgDaysToComplete));
}
