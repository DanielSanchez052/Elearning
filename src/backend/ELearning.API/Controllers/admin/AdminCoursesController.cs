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
[Route("api/admin/courses")]
[Authorize(Roles = "instructor,admin,superadmin")]
public class AdminCoursesController(
    IQueryHandler<GetAdminCourseListQuery, PagedResult<CourseSummaryDto>> getAdminListHandler,
    ICommandHandler<CreateCourseCommand, Guid> createCourseHandler,
    ICommandHandler<UpdateCourseCommand> updateCourseHandler,
    ICommandHandler<ToggleCourseStatusCommand> toggleStatusHandler,
    ICommandHandler<DeleteCourseCommand> deleteCourseHandler,
    ICommandHandler<AssignCourseCountriesCommand> assignCountriesHandler,
    ICommandHandler<CreateLessonCommand, Guid> createLessonHandler,
    ICommandHandler<UpdateLessonCommand> updateLessonHandler,
    ICommandHandler<DeleteLessonCommand> deleteLessonHandler,
    ICommandHandler<ReorderLessonsCommand> reorderLessonsHandler
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAdminList(
        [FromQuery] Guid? instructorId = null,
        [FromQuery] int? countryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var requesterRole = User.GetRole();

        var effectiveInstructorId = requesterRole == "instructor"
            ? User.GetUserId()
            : instructorId;

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

    [HttpPost]
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
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
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

    [HttpPatch("{id:guid}/toggle-status")]
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

    [HttpDelete("{id:guid}")]
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

    [HttpPut("{id:guid}/countries")]
    public async Task<IActionResult> AssignCountries(Guid id, [FromBody] AssignCountriesRequest request)
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

    [HttpPost("{courseId:guid}/lessons")]
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

    [HttpPut("{courseId:guid}/lessons/{lessonId:guid}")]
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

    [HttpDelete("{courseId:guid}/lessons/{lessonId:guid}")]
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

    [HttpPatch("{courseId:guid}/lessons/reorder")]
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
