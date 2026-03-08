using ELearning.API.Extensions;
using ELearning.API.Models;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Application.Features.Quizzes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesController(
    IQueryHandler<GetLessonQuizzesQuery, IReadOnlyList<QuizQuestionDto>> getLessonQuizzesHandler,
    IQueryHandler<GetCourseExamQuery, IReadOnlyList<QuizQuestionDto>> getCourseExamHandler,
    IQueryHandler<GetUserQuizResultsQuery, IReadOnlyList<QuizAttemptDto>> getUserResultsHandler,
    ICommandHandler<SubmitQuizCommand, QuizResultDto> submitQuizHandler
) : ControllerBase
{
    // ── GET /api/quizzes/lessons/{lessonId} ────────────────────────────────────
    // Obtener todas las preguntas de una lección

    [HttpGet("lessons/{lessonId:guid}")]
    public async Task<IActionResult> GetLessonQuizzes(Guid lessonId, CancellationToken ct)
    {
        var result = await getLessonQuizzesHandler.HandleAsync(new GetLessonQuizzesQuery(lessonId), ct);
        return this.ToActionResult(result);
    }

    // ── GET /api/quizzes/courses/{courseId}/exam ───────────────────────────────
    // Obtener el examen final del curso

    [HttpGet("courses/{courseId:guid}/exam")]
    public async Task<IActionResult> GetCourseExam(Guid courseId, CancellationToken ct)
    {
        var result = await getCourseExamHandler.HandleAsync(new GetCourseExamQuery(courseId), ct);
        return this.ToActionResult(result);
    }

    // ── POST /api/quizzes/lessons/{lessonId}/submit ────────────────────────────
    // Enviar respuestas de un quiz de lección

    [HttpPost("lessons/{lessonId:guid}/submit")]
    public async Task<IActionResult> SubmitLessonQuiz(Guid lessonId, [FromBody] SubmitQuizRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var cmd = new SubmitQuizCommand(userId, lessonId, null, request.SelectedOptionIds);
        var result = await submitQuizHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }

    // ── POST /api/quizzes/courses/{courseId}/exam/submit ───────────────────────
    // Enviar respuestas del examen del curso

    [HttpPost("courses/{courseId:guid}/exam/submit")]
    public async Task<IActionResult> SubmitCourseExam(Guid courseId, [FromBody] SubmitQuizRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var cmd = new SubmitQuizCommand(userId, null, courseId, request.SelectedOptionIds);
        var result = await submitQuizHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }

    // ── GET /api/quizzes/lessons/{lessonId}/results ────────────────────────────
    // Ver historial de intentos de un quiz de lección

    [HttpGet("lessons/{lessonId:guid}/results")]
    public async Task<IActionResult> GetLessonResults(Guid lessonId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await getUserResultsHandler.HandleAsync(
            new GetUserQuizResultsQuery(userId, lessonId, null), ct);
        return this.ToActionResult(result);
    }

    // ── GET /api/quizzes/courses/{courseId}/exam/results ───────────────────────
    // Ver historial de intentos del examen del curso

    [HttpGet("courses/{courseId:guid}/exam/results")]
    public async Task<IActionResult> GetCourseExamResults(Guid courseId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await getUserResultsHandler.HandleAsync(
            new GetUserQuizResultsQuery(userId, null, courseId), ct);
        return this.ToActionResult(result);
    }
}
