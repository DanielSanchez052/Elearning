namespace ELearning.Application.Features.Reports.Common;

/// <summary>Cálculos de los reportes, separados de las consultas para poder probarlos.</summary>
public static class ReportMath
{
    /// <summary>Porcentaje (0–100) con un decimal; null si no hay base sobre la que calcularlo.</summary>
    public static double? Percentage(int part, int total) =>
        total <= 0 ? null : Math.Round(part * 100.0 / total, 1);

    /// <summary>Días con un decimal; null si no hay datos.</summary>
    public static double? Days(double? days) =>
        days is null ? null : Math.Round(days.Value, 1);
}
