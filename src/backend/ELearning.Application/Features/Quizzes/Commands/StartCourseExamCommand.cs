using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record StartCourseExamCommand(
    Guid UserId,
    Guid CourseId
) : ICommand<StartCourseExamResultDto>;

/// <summary>
/// Starts (or resumes) a course final-exam attempt. Applies the same eligibility checks as
/// GetCourseExamHandler plus SubmitQuizHandler's attempt rules. An open session is returned
/// unchanged, so abandoning and coming back keeps the original StartedAt and does not use
/// up an attempt (attempts are counted from submitted results, not from sessions).
/// </summary>
public sealed class StartCourseExamHandler : ICommandHandler<StartCourseExamCommand, StartCourseExamResultDto>
{
    private readonly IQuizRepository _quizzes;
    private readonly IEnrollmentRepository _enrollments;

    public StartCourseExamHandler(IQuizRepository quizzes, IEnrollmentRepository enrollments)
    {
        _quizzes = quizzes;
        _enrollments = enrollments;
    }

    public async Task<Result<StartCourseExamResultDto>> HandleAsync(StartCourseExamCommand cmd, CancellationToken ct = default)
    {
        // 1. VALIDACIONES (mismas que GetCourseExamHandler)
        if (cmd.UserId == Guid.Empty)
            return Result.ValidationFailure<StartCourseExamResultDto>("UserId es requerido");

        if (cmd.CourseId == Guid.Empty)
            return Result.ValidationFailure<StartCourseExamResultDto>("CourseId es requerido");

        var enrollment = await _enrollments.GetByUserAndCourseAsync(cmd.UserId, cmd.CourseId, ct);
        if (enrollment is null)
            return Result.Forbidden<StartCourseExamResultDto>("No estás inscrito en este curso");

        if (!enrollment.IsActive)
            return Result.Forbidden<StartCourseExamResultDto>("No tienes una inscripción activa en este curso");

        var completedRequiredIds = enrollment.LessonProgress
            .Where(p => p.IsCompleted)
            .Select(p => p.LessonId)
            .ToHashSet();

        var missingRequiredLessons = enrollment.Course.Lessons
            .Where(l => l.IsRequired)
            .Select(l => l.Id)
            .Where(id => !completedRequiredIds.Contains(id))
            .ToList();

        if (missingRequiredLessons.Count > 0)
            return Result.Forbidden<StartCourseExamResultDto>(
                "Debes completar todas las lecciones requeridas antes de presentar el examen final.");

        var questions = await _quizzes.GetQuestionsByCourseAsync(cmd.CourseId, ct);
        if (questions.Count == 0)
            return Result.NotFound<StartCourseExamResultDto>("No hay preguntas en este quiz");

        // 2. REGLAS DE INTENTOS (mismas que SubmitQuizHandler)
        var maxAttempts = questions.First().MaxAttempts;
        var latestResult = await _quizzes.GetLatestCourseExamResultAsync(cmd.UserId, cmd.CourseId, ct);

        if (latestResult is not null)
        {
            if (latestResult.IsPassed)
                return Result.ValidationFailure<StartCourseExamResultDto>("Ya aprobaste esta evaluación. No requiere más intentos.");

            if (latestResult.AttemptNumber >= maxAttempts)
                return Result.ValidationFailure<StartCourseExamResultDto>($"Alcanzaste el máximo de {maxAttempts} intentos para esta evaluación.");
        }

        var attemptNumber = (latestResult?.AttemptNumber ?? 0) + 1;

        // 3. REANUDAR O CREAR LA SESIÓN
        var session = await _quizzes.GetOpenExamSessionAsync(cmd.UserId, cmd.CourseId, ct);

        if (session is not null && session.AttemptNumber != attemptNumber)
        {
            // Sesión abierta de un intento que ya no es el actual: cerrarla para que no bloquee
            // el envío (SubmitQuizHandler responde Conflict ante una sesión desactualizada).
            session.MarkSubmitted();
            await _quizzes.SaveChangesAsync(ct);
            session = null;
        }

        if (session is null)
        {
            var newSession = ExamSession.Start(cmd.UserId, cmd.CourseId, attemptNumber);

            if (await _quizzes.TryAddExamSessionAsync(newSession, ct))
            {
                session = newSession;
            }
            else
            {
                // Otra solicitud abrió la sesión al mismo tiempo: devolver esa.
                session = await _quizzes.GetOpenExamSessionAsync(cmd.UserId, cmd.CourseId, ct);
                if (session is null)
                    return Result.Conflict<StartCourseExamResultDto>(
                        "No se pudo iniciar el examen por una solicitud simultánea. Intenta nuevamente.");
            }
        }

        return Result.Success(new StartCourseExamResultDto(
            SessionId: session.Id,
            AttemptNumber: session.AttemptNumber,
            StartedAt: session.StartedAt,
            Questions: MapQuestions(questions)));
    }

    // Misma forma segura para el estudiante que GetCourseExamHandler (sin IsCorrect).
    private static IReadOnlyList<QuizQuestionDto> MapQuestions(IReadOnlyList<QuizQuestion> questions) =>
        questions
            .Select(q => new QuizQuestionDto(
                q.Id,
                q.QuestionText,
                (int)q.Type,
                q.IsRequired,
                q.PassScore,
                q.MaxAttempts,
                q.OrderIndex,
                q.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select(o => new QuizOptionDto(o.Id, o.OptionText, o.OrderIndex))
                    .ToList(),
                q.LessonId,
                q.CourseId
            ))
            .ToList();
}
