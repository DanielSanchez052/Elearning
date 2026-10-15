using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Reports.Common;
using ELearning.Application.Features.Reports.DTOs;
using ELearning.Application.Features.Reports.Queries;
using ELearning.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers.admin;

/// <summary>
/// Reportes del panel de administración. El acceso por rol y por país lo decide cada handler
/// (ReportAccess / CourseAccess); aquí solo se exige ser staff.
/// </summary>
[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = RoleNames.Staff)]
public class AdminReportsController(
    IQueryHandler<GetOverviewReportQuery, OverviewReportDto> overviewHandler
) : ControllerBase
{
    // GET /api/admin/reports/overview?period=30d&countryId=1
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(
        [FromQuery] string? period = null,
        [FromQuery] int? countryId = null)
    {
        if (!ReportPeriods.TryParse(period, out var reportPeriod))
            return this.ToActionResult(Result.ValidationFailure<OverviewReportDto>(InvalidPeriodMessage));

        var query = new GetOverviewReportQuery(reportPeriod, countryId, User.GetRole(), User.GetCountryId());
        var result = await overviewHandler.HandleAsync(query, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    private const string InvalidPeriodMessage = "Periodo inválido. Usa 7d, 30d, 90d o all.";
}
