namespace ELearning.Application.Features.Reports.Common;

/// <summary>Periodo de los reportes: últimos 7, 30 o 90 días, o todo el histórico.</summary>
public enum ReportPeriod
{
    Last7Days,
    Last30Days,
    Last90Days,
    AllTime
}

public static class ReportPeriods
{
    public const ReportPeriod Default = ReportPeriod.Last30Days;

    /// <summary>Valores aceptados en la query string: <c>7d</c>, <c>30d</c>, <c>90d</c> y <c>all</c>.</summary>
    public static bool TryParse(string? value, out ReportPeriod period)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case null or "":
                period = Default;
                return true;
            case "7d": period = ReportPeriod.Last7Days; return true;
            case "30d": period = ReportPeriod.Last30Days; return true;
            case "90d": period = ReportPeriod.Last90Days; return true;
            case "all": period = ReportPeriod.AllTime; return true;
            default:
                period = Default;
                return false;
        }
    }

    public static string ToCode(this ReportPeriod period) => period switch
    {
        ReportPeriod.Last7Days => "7d",
        ReportPeriod.Last30Days => "30d",
        ReportPeriod.Last90Days => "90d",
        _ => "all"
    };

    /// <summary>Inicio del periodo; null para todo el histórico.</summary>
    public static DateTime? Since(this ReportPeriod period, DateTime utcNow) => period switch
    {
        ReportPeriod.Last7Days => utcNow.AddDays(-7),
        ReportPeriod.Last30Days => utcNow.AddDays(-30),
        ReportPeriod.Last90Days => utcNow.AddDays(-90),
        _ => null
    };
}
