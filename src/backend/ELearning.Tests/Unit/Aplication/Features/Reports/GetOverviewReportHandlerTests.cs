using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Reports.Common;
using ELearning.Application.Features.Reports.DTOs;
using ELearning.Application.Features.Reports.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.ReadModels.Reports;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Reports;

public class GetOverviewReportHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private const int Colombia = 1;

    private readonly Mock<IReportRepository> _reportsMock = new();
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly GetOverviewReportHandler _handler;

    public GetOverviewReportHandlerTests()
    {
        _handler = new GetOverviewReportHandler(_reportsMock.Object, _countriesMock.Object, new FixedTimeProvider(Now));

        var colombia = Country.Create("CO", "Colombia");
        Helpers.SetPrivate(colombia, nameof(Country.Id), Colombia);
        _countriesMock.Setup(r => r.GetByIdAsync(Colombia, default)).ReturnsAsync(colombia);

        var counts = new ReportCounts(
            Students: 10, ActiveStudents: 4, Enrollments: 8, EnrollmentsCompleted: 2,
            Completions: 3, AvgDaysToComplete: 4.26);
        _reportsMock.Setup(r => r.GetCountsAsync(It.IsAny<ReportFilter>(), default)).ReturnsAsync(counts);
        _reportsMock
            .Setup(r => r.GetTopCoursesAsync(It.IsAny<ReportFilter>(), GetOverviewReportHandler.PopularCoursesCount, default))
            .ReturnsAsync([new CourseEnrollmentCounts(Guid.NewGuid(), "Excel", Enrollments: 3, EnrollmentsCompleted: 1)]);
        _reportsMock
            .Setup(r => r.GetCountsByCountryAsync(It.IsAny<ReportFilter>(), default))
            .ReturnsAsync([new CountryReportCounts(Colombia, "Colombia", counts)]);
    }

    private Task<Result<OverviewReportDto>> Run(
        string role, int? countryId = null, ReportPeriod period = ReportPeriod.Last30Days) =>
        _handler.HandleAsync(new GetOverviewReportQuery(period, countryId, role, Colombia));

    [Fact]
    public async Task SuperAdminWithoutCountry_SeesWholePlatformAndCountryComparison()
    {
        var result = await Run("superadmin");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.CountryId);
        Assert.Single(result.Value.Countries!);
        _reportsMock.Verify(r => r.GetCountsAsync(
            It.Is<ReportFilter>(f => f.CountryId == null && f.Since == Now.AddDays(-30)), default), Times.Once);
    }

    [Fact]
    public async Task SuperAdminWithCountry_IsScopedAndHasNoComparison()
    {
        var result = await Run("superadmin", Colombia);

        Assert.True(result.IsSuccess);
        Assert.Equal("Colombia", result.Value.CountryName);
        Assert.Null(result.Value.Countries);
        _reportsMock.Verify(r => r.GetCountsByCountryAsync(It.IsAny<ReportFilter>(), default), Times.Never);
    }

    [Fact]
    public async Task Admin_IsAlwaysScopedToOwnCountry_EvenIfAskingForAnother()
    {
        var result = await Run("admin", countryId: 52);

        Assert.True(result.IsSuccess);
        Assert.Equal(Colombia, result.Value.CountryId);
        Assert.Null(result.Value.Countries);
        _reportsMock.Verify(r => r.GetCountsAsync(It.Is<ReportFilter>(f => f.CountryId == Colombia), default), Times.Once);
    }

    [Theory]
    [InlineData("instructor")]
    [InlineData("student")]
    public async Task NonAdmins_AreForbidden(string role)
    {
        var result = await Run(role);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        _reportsMock.Verify(r => r.GetCountsAsync(It.IsAny<ReportFilter>(), default), Times.Never);
    }

    [Fact]
    public async Task MapsKpisAndRates()
    {
        var result = await Run("superadmin", Colombia, ReportPeriod.AllTime);

        var kpis = result.Value.Kpis;
        Assert.Equal("all", result.Value.Period);
        Assert.Null(result.Value.Since);
        Assert.Equal(10, kpis.Students);
        Assert.Equal(4, kpis.ActiveStudents);
        Assert.Equal(3, kpis.Completions);
        Assert.Equal(25.0, kpis.CompletionRate);      // 2 de las 8 inscripciones del periodo
        Assert.Equal(4.3, kpis.AvgDaysToComplete);
        Assert.Equal(33.3, result.Value.PopularCourses[0].CompletionRate);
    }

    [Fact]
    public async Task UnknownCountry_ReturnsNotFound()
    {
        var result = await Run("superadmin", countryId: 99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
