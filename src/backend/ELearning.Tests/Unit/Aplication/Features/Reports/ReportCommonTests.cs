using ELearning.Application.Features.Reports.Common;

namespace ELearning.Tests.Unit.Aplication.Features.Reports;

public class ReportCommonTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("7d", ReportPeriod.Last7Days)]
    [InlineData("30d", ReportPeriod.Last30Days)]
    [InlineData("90D", ReportPeriod.Last90Days)]
    [InlineData("all", ReportPeriod.AllTime)]
    [InlineData(null, ReportPeriod.Last30Days)]   // por defecto
    [InlineData("", ReportPeriod.Last30Days)]
    public void TryParse_AcceptsKnownCodes(string? code, ReportPeriod expected)
    {
        Assert.True(ReportPeriods.TryParse(code, out var period));
        Assert.Equal(expected, period);
    }

    [Theory]
    [InlineData("1y")]
    [InlineData("30")]
    public void TryParse_RejectsUnknownCodes(string code) =>
        Assert.False(ReportPeriods.TryParse(code, out _));

    [Theory]
    [InlineData(ReportPeriod.Last7Days, 7)]
    [InlineData(ReportPeriod.Last30Days, 30)]
    [InlineData(ReportPeriod.Last90Days, 90)]
    public void Since_SubtractsTheDays(ReportPeriod period, int days) =>
        Assert.Equal(Now.AddDays(-days), period.Since(Now));

    [Fact]
    public void Since_AllTime_IsNull() =>
        Assert.Null(ReportPeriod.AllTime.Since(Now));

    [Theory]
    [InlineData(ReportPeriod.Last7Days, "7d")]
    [InlineData(ReportPeriod.AllTime, "all")]
    public void ToCode_RoundTrips(ReportPeriod period, string code)
    {
        Assert.Equal(code, period.ToCode());
        Assert.True(ReportPeriods.TryParse(period.ToCode(), out var parsed));
        Assert.Equal(period, parsed);
    }

    [Theory]
    [InlineData(1, 3, 33.3)]
    [InlineData(2, 2, 100.0)]
    [InlineData(0, 5, 0.0)]
    public void Percentage_RoundsToOneDecimal(int part, int total, double expected) =>
        Assert.Equal(expected, ReportMath.Percentage(part, total));

    [Fact]
    public void Percentage_WithoutBase_IsNull() =>
        Assert.Null(ReportMath.Percentage(0, 0));

    [Fact]
    public void Days_RoundsAndKeepsNull()
    {
        Assert.Equal(2.3, ReportMath.Days(2.345));
        Assert.Null(ReportMath.Days(null));
    }
}
