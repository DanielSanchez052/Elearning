using ELearning.API.Extensions;
using ELearning.API.Models;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Commands;
using ELearning.Application.Features.Courses.DTOs;
using ELearning.Application.Features.Courses.Queries;
using ELearning.Application.Features.Lessons.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(
    // Catálogo público
    IQueryHandler<GetCourseCatalogQuery, PagedResult<CourseSummaryDto>> getCatalogHandler,
    IQueryHandler<GetCourseDetailQuery, CourseDetailDto> getCourseDetailHandler,
    // Admin
    IQueryHandler<GetAdminCourseListQuery, PagedResult<CourseSummaryDto>> getAdminListHandler,
    // Commands
    ICommandHandler<CreateCourseCommand, Guid> createCourseHandler,
    ICommandHandler<UpdateCourseCommand> updateCourseHandler,
    ICommandHandler<ToggleCourseStatusCommand> toggleStatusHandler,
    ICommandHandler<DeleteCourseCommand> deleteCourseHandler,
    ICommandHandler<AssignCourseCountriesCommand> assignCountriesHandler,
    // Lecciones
    ICommandHandler<CreateLessonCommand, Guid> createLessonHandler,
    ICommandHandler<UpdateLessonCommand> updateLessonHandler,
    ICommandHandler<DeleteLessonCommand> deleteLessonHandler,
    ICommandHandler<ReorderLessonsCommand> reorderLessonsHandler
) : ControllerBase
{
    // ── CATÁLOGO PÚBLICO ──────────────────────────────────────────────────────

    /// GET api/courses
    /// Requiere autenticación para filtrar por el país del usuario
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCatalog([FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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

    /// GET api/courses/{id}
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetCourseDetail(Guid id)
    {
        var result = await getCourseDetailHandler.HandleAsync(
            new GetCourseDetailQuery(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    // ── ADMIN — LISTA COMPLETA ────────────────────────────────────────────────

    /// GET api/courses/admin?instructorId=...&countryId=1&isActive=true&search=...
    [HttpGet("admin")]
    [Authorize(Roles = "admin,superadmin,instructor")]
    public async Task<IActionResult> GetAdminList(
        [FromQuery] Guid? instructorId = null,
        [FromQuery] int? countryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var requesterRole = User.GetRole();

        // Instructor solo ve sus propios cursos
        var effectiveInstructorId = requesterRole == "instructor"
            ? User.GetUserId()
            : instructorId;

        // Admin solo ve cursos de su país
        var effectiveCountryId = User.IsAdmin() && !User.IsSuperAdmin()
            ? User.GetCountryId()
            : countryId;

        var query = new GetAdminCourseListQuery(
            InstructorId: effectiveInstructorId,
            CountryId: effectiveCountryId,
            IsActive: isActive,
            Search: search,
            Page: page,
            PageSize: pageSize
        );

        var result = await getAdminListHandler.HandleAsync(query, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    // ── COMMANDS DE CURSOS ────────────────────────────────────────────────────

    /// POST api/courses
    [HttpPost]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var cmd = new CreateCourseCommand(
            Title: request.Title,
            Description: request.Description,
            ThumbnailUrl: request.ThumbnailUrl,
            IsGlobal: request.IsGlobal,
            CreatedBy: User.GetUserId(),
            CreatorCountryId: User.GetCountryId()
        );

        var result = await createCourseHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToCreatedResult(result, nameof(GetCourseDetail), new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// PUT api/courses/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var cmd = new UpdateCourseCommand(
            CourseId: id,
            Title: request.Title,
            Description: request.Description,
            ThumbnailUrl: request.ThumbnailUrl,
            IsGlobal: request.IsGlobal,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await updateCourseHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// PATCH api/courses/{id}/toggle-status
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> ToggleCourseStatus(Guid id)
    {
        var cmd = new ToggleCourseStatusCommand(
            CourseId: id,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await toggleStatusHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// DELETE api/courses/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        var cmd = new DeleteCourseCommand(
            CourseId: id,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await deleteCourseHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// PUT api/courses/{id}/countries
    [HttpPut("{id:guid}/countries")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> AssignCountries(
        Guid id,
        [FromBody] AssignCountriesRequest request)
    {
        var cmd = new AssignCourseCountriesCommand(
            CourseId: id,
            CountryIds: request.CountryIds,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await assignCountriesHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    // ── LECCIONES ─────────────────────────────────────────────────────────────

    /// POST api/courses/{courseId}/lessons
    [HttpPost("{courseId:guid}/lessons")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> CreateLesson(Guid courseId, [FromBody] CreateLessonRequest request)
    {
        var cmd = new CreateLessonCommand(
            CourseId: courseId,
            Title: request.Title,
            Type: request.Type,
            ContentUrl: request.ContentUrl,
            IsRequired: request.IsRequired,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await createLessonHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// PUT api/courses/{courseId}/lessons/{lessonId}
    [HttpPut("{courseId:guid}/lessons/{lessonId:guid}")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> UpdateLesson(Guid courseId, Guid lessonId, [FromBody] UpdateLessonRequest request)
    {
        var cmd = new UpdateLessonCommand(
            LessonId: lessonId,
            Title: request.Title,
            ContentUrl: request.ContentUrl,
            IsRequired: request.IsRequired,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await updateLessonHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// DELETE api/courses/{courseId}/lessons/{lessonId}
    [HttpDelete("{courseId:guid}/lessons/{lessonId:guid}")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> DeleteLesson(Guid courseId, Guid lessonId)
    {
        var cmd = new DeleteLessonCommand(
            LessonId: lessonId,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await deleteLessonHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// PATCH api/courses/{courseId}/lessons/reorder
    [HttpPatch("{courseId:guid}/lessons/reorder")]
    [Authorize(Roles = "instructor,admin,superadmin")]
    public async Task<IActionResult> ReorderLessons(Guid courseId, [FromBody] ReorderLessonsRequest request)
    {
        var cmd = new ReorderLessonsCommand(
            CourseId: courseId,
            Orders: request.Orders,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await reorderLessonsHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }
}