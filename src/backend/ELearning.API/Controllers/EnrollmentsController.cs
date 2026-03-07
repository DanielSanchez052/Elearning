using ELearning.API.Extensions;
using ELearning.API.Models;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.Commands;
using ELearning.Application.Features.Enrollments.DTOs;
using ELearning.Application.Features.Enrollments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController(
    ICommandHandler<EnrollInCourseCommand, Guid> enrollHandler,
    ICommandHandler<MarkLessonCompleteCommand, MarkLessonCompleteResult> markCompleteHandler,
    IQueryHandler<GetMyEnrollmentsQuery, IReadOnlyList<EnrollmentSummaryDto>> myEnrollmentsHandler,
    IQueryHandler<GetCourseProgressQuery, CourseProgressDto> progressHandler
) : ControllerBase
{
    // ── POST /api/enrollments ─────────────────────────────────────────────────
    // Inscribir al usuario autenticado en un curso

    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var command = new EnrollInCourseCommand(userId, request.CourseId);
        var result = await enrollHandler.HandleAsync(command, ct);

        return this.ToCreatedResult(result, nameof(GetProgress), new { courseId = request.CourseId });
    }

    // ── GET /api/enrollments/me ───────────────────────────────────────────────
    // Lista todos los cursos inscritos con progreso resumido

    [HttpGet("me")]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await myEnrollmentsHandler.HandleAsync(new GetMyEnrollmentsQuery(userId), ct);
        return this.ToActionResult(result);
    }

    // ── GET /api/enrollments/me/courses/{courseId} ────────────────────────────
    // Progreso detallado (lección por lección) de un curso inscripto

    [HttpGet("me/courses/{courseId:guid}")]
    public async Task<IActionResult> GetProgress(Guid courseId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await progressHandler.HandleAsync(new GetCourseProgressQuery(userId, courseId), ct);
        return this.ToActionResult(result);
    }

    // ── POST /api/enrollments/me/courses/{courseId}/lessons/{lessonId}/complete
    // Marcar una lección como completada

    [HttpPost("me/courses/{courseId:guid}/lessons/{lessonId:guid}/complete")]
    public async Task<IActionResult> MarkLessonComplete(
        Guid courseId,
        Guid lessonId,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var command = new MarkLessonCompleteCommand(userId, courseId, lessonId);
        var result = await markCompleteHandler.HandleAsync(command, ct);
        return this.ToActionResult(result);
    }
}