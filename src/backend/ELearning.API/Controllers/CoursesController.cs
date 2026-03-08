using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.DTOs;
using ELearning.Application.Features.Courses.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController(
    IQueryHandler<GetCourseCatalogQuery, PagedResult<CourseSummaryDto>> getCatalogHandler,
    IQueryHandler<GetCourseDetailQuery, CourseDetailDto> getCourseDetailHandler
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCatalog(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetCourseCatalogQuery(
            CountryId: User.GetCountryId(),
            Search: search,
            Page: page,
            PageSize: pageSize
        );

        var result = await getCatalogHandler.HandleAsync(query, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCourseDetail(Guid id)
    {
        var result = await getCourseDetailHandler.HandleAsync(
            new GetCourseDetailQuery(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }
}
