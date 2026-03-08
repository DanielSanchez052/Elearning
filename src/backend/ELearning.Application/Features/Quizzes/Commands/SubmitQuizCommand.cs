using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record SubmitQuizCommand(
    Guid UserId,
    Guid? LessonId,
    Guid? CourseId,
    IReadOnlyList<Guid> SelectedOptionIds
) : ICommand<QuizResultDto>;

public sealed class SubmitQuizHandler : ICommandHandler<SubmitQuizCommand, QuizResultDto>
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly ILessonRepository _lessons;
    private readonly ICourseRepository _courses;
    private readonly IQuizRepository _quizzes;

    public SubmitQuizHandler(
        IEnrollmentRepository enrollments,
        ILessonRepository lessons,
        ICourseRepository courses,
        IQuizRepository quizzes)
    {
        _enrollments = enrollments;
        _lessons = lessons;
        _courses = courses;
        _quizzes = quizzes;
    }

    public async Task<Result<QuizResultDto>> HandleAsync(SubmitQuizCommand cmd, CancellationToken ct = default)
    {
        // 1. VALIDACIONES
        if (cmd.UserId == Guid.Empty)
            return Result.ValidationFailure<QuizResultDto>("UserId es requerido");

        if (cmd.SelectedOptionIds == null || cmd.SelectedOptionIds.Count == 0)
            return Result.ValidationFailure<QuizResultDto>("Debe seleccionar al menos una opción");

        if ((cmd.LessonId == null || cmd.LessonId == Guid.Empty) &&
            (cmd.CourseId == null || cmd.CourseId == Guid.Empty))
            return Result.ValidationFailure<QuizResultDto>("Debe proporcionar LessonId o CourseId");

        Guid courseId = Guid.Empty;
        Lesson? lessonContext = null;
        CourseEnrollment? enrollment = null;

        if (cmd.LessonId.HasValue && cmd.LessonId != Guid.Empty)
        {
            var lesson = await _lessons.GetByIdAsync(cmd.LessonId.Value, ct);
            if (lesson is null)
                return Result.NotFound<QuizResultDto>("Lección no encontrada");

            lessonContext = lesson;
            courseId = lesson.CourseId;

            enrollment = await _enrollments.GetByUserAndCourseAsync(cmd.UserId, courseId, ct);
            if (enrollment is null)
                return Result.Forbidden<QuizResultDto>("No estás inscrito en este curso");
        }
        else if (cmd.CourseId.HasValue && cmd.CourseId != Guid.Empty)
        {
            var course = await _courses.GetByIdAsync(cmd.CourseId.Value, ct);
            if (course is null)
                return Result.NotFound<QuizResultDto>("Curso no encontrado");

            courseId = cmd.CourseId.Value;

            enrollment = await _enrollments.GetByUserAndCourseAsync(cmd.UserId, courseId, ct);
            if (enrollment is null)
                return Result.Forbidden<QuizResultDto>("No estás inscrito en este curso");
        }

        if (enrollment is null)
            return Result.Forbidden<QuizResultDto>("No se pudo validar tu inscripción para esta evaluación");

        if (!enrollment.IsActive)
            return Result.Forbidden<QuizResultDto>("No tienes una inscripción activa en este curso");

        var completedRequiredIds = enrollment.LessonProgress
            .Where(p => p.IsCompleted)
            .Select(p => p.LessonId)
            .ToHashSet();

        if (lessonContext is not null)
        {
            var missingRequiredBeforeLesson = enrollment.Course.Lessons
                .Where(l => l.IsRequired && l.OrderIndex < lessonContext.OrderIndex)
                .Select(l => l.Id)
                .Where(id => !completedRequiredIds.Contains(id))
                .ToList();

            if (missingRequiredBeforeLesson.Count > 0)
                return Result.Forbidden<QuizResultDto>(
                    "Debes completar las lecciones requeridas previas antes de presentar esta evaluación.");
        }
        else
        {
            var missingRequiredLessons = enrollment.Course.Lessons
                .Where(l => l.IsRequired)
                .Select(l => l.Id)
                .Where(id => !completedRequiredIds.Contains(id))
                .ToList();

            if (missingRequiredLessons.Count > 0)
                return Result.Forbidden<QuizResultDto>(
                    "Debes completar todas las lecciones requeridas antes de presentar el examen final.");
        }

        // 2. OBTENER PREGUNTAS
        IReadOnlyList<QuizQuestion> questions;

        if (cmd.LessonId.HasValue && cmd.LessonId != Guid.Empty)
        {
            questions = await _quizzes.GetQuestionsByLessonAsync(cmd.LessonId.Value, ct);
        }
        else
        {
            questions = await _quizzes.GetQuestionsByCourseAsync(cmd.CourseId!.Value, ct);
        }

        if (questions.Count == 0)
            return Result.NotFound<QuizResultDto>("No hay preguntas en este quiz");

        if (cmd.SelectedOptionIds.Count != questions.Count)
            return Result.ValidationFailure<QuizResultDto>($"Debe responder todas las {questions.Count} preguntas");

        // 3. VALIDAR REGLAS DE INTENTOS
        var maxAttempts = questions.First().MaxAttempts;
        UserQuizResult? latestResult;

        if (cmd.LessonId.HasValue && cmd.LessonId != Guid.Empty)
        {
            latestResult = await _quizzes.GetLatestLessonResultAsync(cmd.UserId, cmd.LessonId.Value, ct);
        }
        else
        {
            latestResult = await _quizzes.GetLatestCourseExamResultAsync(cmd.UserId, cmd.CourseId!.Value, ct);
        }

        if (latestResult is not null)
        {
            if (latestResult.IsPassed)
                return Result.ValidationFailure<QuizResultDto>("Ya aprobaste esta evaluación. No requiere más intentos.");

            if (latestResult.AttemptNumber >= maxAttempts)
                return Result.ValidationFailure<QuizResultDto>($"Alcanzaste el máximo de {maxAttempts} intentos para esta evaluación.");
        }

        var attemptNumber = (latestResult?.AttemptNumber ?? 0) + 1;

        // 4. CALCULAR SCORE
        int correctAnswers = 0;
        var questionsList = questions.ToList();

        for (int i = 0; i < questionsList.Count; i++)
        {
            var question = questionsList[i];
            var selectedOptionId = cmd.SelectedOptionIds[i];
            var selectedOption = await _quizzes.GetOptionByIdAsync(selectedOptionId, ct);

            if (selectedOption is null || selectedOption.QuestionId != question.Id)
                return Result.ValidationFailure<QuizResultDto>($"La respuesta seleccionada para la pregunta {i + 1} no es válida.");

            if (selectedOption.IsCorrect)
            {
                correctAnswers++;
            }

            // Guardar intento
            var attempt = UserQuizAttempt.Create(cmd.UserId, question.Id, selectedOptionId, attemptNumber);
            await _quizzes.CreateAttemptAsync(attempt, ct);
        }

        decimal score = (decimal)(correctAnswers * 100) / questionsList.Count;
        decimal passScore = questionsList.First().PassScore;
        bool isPassed = score >= passScore;

        if (cmd.CourseId.HasValue && cmd.CourseId != Guid.Empty && isPassed)
        {
            var requiredLessonIds = enrollment.Course.Lessons
                .Where(l => l.IsRequired)
                .Select(l => l.Id)
                .ToList();

            enrollment.TryComplete(requiredLessonIds);
        }

        // 5. CREAR RESULTADO
        UserQuizResult result;
        if (cmd.LessonId.HasValue)
        {
            result = UserQuizResult.Create(cmd.UserId, cmd.LessonId, null, attemptNumber, score, passScore);
        }
        else
        {
            result = UserQuizResult.Create(cmd.UserId, null, cmd.CourseId, attemptNumber, score, passScore);
        }

        await _quizzes.CreateResultAsync(result, ct);
        await _quizzes.SaveChangesAsync(ct);

        // 6. CREAR DTO DE RESPUESTA
        var resultDto = new QuizResultDto(
            Score: score,
            IsPassed: isPassed,
            PassScore: passScore,
            TotalQuestions: questions.Count,
            CorrectAnswers: correctAnswers,
            AttemptNumber: attemptNumber,
            MaxAttempts: maxAttempts,
            Feedback: isPassed 
                ? $"¡Felicidades! Pasaste el quiz con {score:F1}%"
                : $"No pasaste el quiz. Obtuviste {score:F1}% y necesitas {passScore}%. Intenta nuevamente.",
            CompletedAt: DateTime.UtcNow
        );

        return resultDto;
    }
}
