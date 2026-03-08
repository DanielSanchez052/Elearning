using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Repositories;

public interface IQuizRepository
{
    // ── Questions ──────────────────────────────────────────────────────────────

    /// <summary>Obtener pregunta por ID</summary>
    Task<QuizQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct = default);

    /// <summary>Obtener todas las preguntas de una lección</summary>
    Task<IReadOnlyList<QuizQuestion>> GetQuestionsByLessonAsync(Guid lessonId, CancellationToken ct = default);

    /// <summary>Obtener todas las preguntas de examen de un curso</summary>
    Task<IReadOnlyList<QuizQuestion>> GetQuestionsByCourseAsync(Guid courseId, CancellationToken ct = default);

    /// <summary>Crear nueva pregunta</summary>
    Task<QuizQuestion> CreateQuestionAsync(QuizQuestion question, CancellationToken ct = default);

    /// <summary>Actualizar pregunta</summary>
    Task UpdateQuestionAsync(QuizQuestion question, CancellationToken ct = default);

    /// <summary>Eliminar pregunta (y todas sus opciones)</summary>
    Task DeleteQuestionAsync(Guid questionId, CancellationToken ct = default);

    // ── Options ────────────────────────────────────────────────────────────────

    /// <summary>Obtener opción por ID</summary>
    Task<QuizOption?> GetOptionByIdAsync(Guid optionId, CancellationToken ct = default);

    /// <summary>Obtener todas las opciones de una pregunta</summary>
    Task<IReadOnlyList<QuizOption>> GetOptionsByQuestionAsync(Guid questionId, CancellationToken ct = default);

    /// <summary>Crear nueva opción</summary>
    Task<QuizOption> CreateOptionAsync(QuizOption option, CancellationToken ct = default);

    /// <summary>Actualizar opción</summary>
    Task UpdateOptionAsync(QuizOption option, CancellationToken ct = default);

    /// <summary>Eliminar opción</summary>
    Task DeleteOptionAsync(Guid optionId, CancellationToken ct = default);

    // ── User Attempts ──────────────────────────────────────────────────────────

    /// <summary>Registrar intento de respuesta del usuario</summary>
    Task<UserQuizAttempt> CreateAttemptAsync(UserQuizAttempt attempt, CancellationToken ct = default);

    /// <summary>Obtener todos los intentos del usuario en una pregunta</summary>
    Task<IReadOnlyList<UserQuizAttempt>> GetAttemptsAsync(Guid userId, Guid questionId, CancellationToken ct = default);

    // ── User Results ───────────────────────────────────────────────────────────

    /// <summary>Crear resultado de quiz</summary>
    Task<UserQuizResult> CreateResultAsync(UserQuizResult result, CancellationToken ct = default);

    /// <summary>Obtener resultado más reciente de una lección</summary>
    Task<UserQuizResult?> GetLatestLessonResultAsync(Guid userId, Guid lessonId, CancellationToken ct = default);

    /// <summary>Obtener resultado más reciente de un examen de curso</summary>
    Task<UserQuizResult?> GetLatestCourseExamResultAsync(Guid userId, Guid courseId, CancellationToken ct = default);

    /// <summary>Obtener todos los intentos de un usuario en una lección</summary>
    Task<IReadOnlyList<UserQuizResult>> GetLessonAttemptsAsync(Guid userId, Guid lessonId, CancellationToken ct = default);

    /// <summary>Obtener todos los intentos de un usuario en examen de curso</summary>
    Task<IReadOnlyList<UserQuizResult>> GetCourseExamAttemptsAsync(Guid userId, Guid courseId, CancellationToken ct = default);

    // ── Persistence ────────────────────────────────────────────────────────────

    /// <summary>Guardar cambios en la BD</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
