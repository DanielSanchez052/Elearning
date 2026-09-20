using ELearning.API.Extensions;
using ELearning.API.Models;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Application.Features.Quizzes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers.admin;

[ApiController]
[Route("api/admin/quizzes")]
[Authorize(Roles = "admin,superadmin,instructor")]
public class AdminQuizzesController(
    ICommandHandler<CreateQuizQuestionCommand, Guid> createQuestionHandler,
    ICommandHandler<UpdateQuizQuestionCommand> updateQuestionHandler,
    ICommandHandler<DeleteQuizQuestionCommand> deleteQuestionHandler,
    ICommandHandler<CreateQuizOptionCommand, Guid> createOptionHandler,
    ICommandHandler<UpdateQuizOptionCommand> updateOptionHandler,
    ICommandHandler<DeleteQuizOptionCommand> deleteOptionHandler,
    IQueryHandler<GetLessonQuizQuestionsAdminQuery, IReadOnlyList<QuizQuestionAdminDto>> getLessonQuizQuestionsHandler,
    IQueryHandler<GetCourseExamQuestionsAdminQuery, IReadOnlyList<QuizQuestionAdminDto>> getCourseExamQuestionsHandler
) : ControllerBase
{
    // ── Questions ──────────────────────────────────────────────────────────────

    // GET /api/admin/quizzes/lessons/{lessonId}
    // Listar preguntas de una lección (sin gating de inscripción — solo staff autorizado)
    [HttpGet("lessons/{lessonId:guid}")]
    public async Task<IActionResult> GetLessonQuizQuestions(Guid lessonId, CancellationToken ct)
    {
        var query = new GetLessonQuizQuestionsAdminQuery(lessonId);
        var result = await getLessonQuizQuestionsHandler.HandleAsync(query, ct);
        return this.ToActionResult(result);
    }

    // GET /api/admin/quizzes/courses/{courseId}/exam
    // Listar preguntas del examen final de un curso (sin gating de inscripción — solo staff autorizado)
    [HttpGet("courses/{courseId:guid}/exam")]
    public async Task<IActionResult> GetCourseExamQuestions(Guid courseId, CancellationToken ct)
    {
        var query = new GetCourseExamQuestionsAdminQuery(courseId);
        var result = await getCourseExamQuestionsHandler.HandleAsync(query, ct);
        return this.ToActionResult(result);
    }

    // POST /api/admin/quizzes/questions
    // Crear pregunta de quiz (por lección o examen de curso)
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuizQuestionRequest request, CancellationToken ct)
    {
        var cmd = new CreateQuizQuestionCommand(
            request.LessonId,
            request.CourseId,
            request.Type,
            request.QuestionText,
            request.PassScore,
            request.MaxAttempts,
            request.IsRequired
        );

        var result = await createQuestionHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }

    // PUT /api/admin/quizzes/questions/{questionId}
    // Actualizar pregunta
    [HttpPut("questions/{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromBody] UpdateQuizQuestionRequest request, CancellationToken ct)
    {
        var cmd = new UpdateQuizQuestionCommand(
            questionId,
            request.QuestionText,
            request.PassScore,
            request.MaxAttempts,
            request.IsRequired
        );

        var result = await updateQuestionHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }

    // DELETE /api/admin/quizzes/questions/{questionId}
    // Eliminar pregunta
    [HttpDelete("questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid questionId, CancellationToken ct)
    {
        var cmd = new DeleteQuizQuestionCommand(questionId);
        var result = await deleteQuestionHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }

    // ── Options ────────────────────────────────────────────────────────────────

    // POST /api/admin/quizzes/questions/{questionId}/options
    // Crear opción para una pregunta
    [HttpPost("questions/{questionId:guid}/options")]
    public async Task<IActionResult> CreateOption(Guid questionId, [FromBody] CreateQuizOptionRequest request, CancellationToken ct)
    {
        var cmd = new CreateQuizOptionCommand(
            questionId,
            request.OptionText,
            request.IsCorrect,
            request.OrderIndex
        );

        var result = await createOptionHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }

    // PUT /api/admin/quizzes/questions/{questionId}/options/{optionId}
    // Actualizar opción
    [HttpPut("questions/{questionId:guid}/options/{optionId:guid}")]
    public async Task<IActionResult> UpdateOption(Guid questionId, Guid optionId, [FromBody] UpdateQuizOptionRequest request, CancellationToken ct)
    {
        var cmd = new UpdateQuizOptionCommand(optionId, request.OptionText, request.IsCorrect);
        var result = await updateOptionHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }

    // DELETE /api/admin/quizzes/questions/{questionId}/options/{optionId}
    // Eliminar opción
    [HttpDelete("questions/{questionId:guid}/options/{optionId:guid}")]
    public async Task<IActionResult> DeleteOption(Guid questionId, Guid optionId, CancellationToken ct)
    {
        var cmd = new DeleteQuizOptionCommand(optionId);
        var result = await deleteOptionHandler.HandleAsync(cmd, ct);
        return this.ToActionResult(result);
    }
}
