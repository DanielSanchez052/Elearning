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

        Guid quizContextId = Guid.Empty;
        Guid courseId = Guid.Empty;

        if (cmd.LessonId.HasValue && cmd.LessonId != Guid.Empty)
        {
            var lesson = await _lessons.GetByIdAsync(cmd.LessonId.Value, ct);
            if (lesson is null)
                return Result.NotFound<QuizResultDto>("Lección no encontrada");

            quizContextId = cmd.LessonId.Value;
            courseId = lesson.CourseId;

            var enrollment = await _enrollments.GetByUserAndCourseAsync(cmd.UserId, courseId, ct);
            if (enrollment is null)
                return Result.Forbidden<QuizResultDto>("No estás inscrito en este curso");
        }
        else if (cmd.CourseId.HasValue && cmd.CourseId != Guid.Empty)
        {
            var course = await _courses.GetByIdAsync(cmd.CourseId.Value, ct);
            if (course is null)
                return Result.NotFound<QuizResultDto>("Curso no encontrado");

            quizContextId = cmd.CourseId.Value;
            courseId = cmd.CourseId.Value;

            var enrollment = await _enrollments.GetByUserAndCourseAsync(cmd.UserId, courseId, ct);
            if (enrollment is null)
                return Result.Forbidden<QuizResultDto>("No estás inscrito en este curso");
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

        // 3. CALCULAR SCORE
        int correctAnswers = 0;
        int attemptNumber = 1;
        var questionsList = questions.ToList();

        for (int i = 0; i < questionsList.Count; i++)
        {
            var question = questionsList[i];
            var selectedOptionId = cmd.SelectedOptionIds[i];
            var selectedOption = await _quizzes.GetOptionByIdAsync(selectedOptionId, ct);

            if (selectedOption is not null && selectedOption.IsCorrect)
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

        // 4. CREAR RESULTADO
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

        // 5. CREAR DTO DE RESPUESTA
        var resultDto = new QuizResultDto(
            Score: score,
            IsPassed: isPassed,
            PassScore: passScore,
            TotalQuestions: questions.Count,
            CorrectAnswers: correctAnswers,
            AttemptNumber: attemptNumber,
            MaxAttempts: questions.First().MaxAttempts,
            Feedback: isPassed 
                ? $"¡Felicidades! Pasaste el quiz con {score:F1}%"
                : $"No pasaste el quiz. Obtuviste {score:F1}% y necesitas {passScore}%. Intenta nuevamente.",
            CompletedAt: DateTime.UtcNow
        );

        return resultDto;
    }
}
