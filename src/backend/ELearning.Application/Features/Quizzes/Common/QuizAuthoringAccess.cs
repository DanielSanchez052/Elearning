using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Common;

/// <summary>
/// Reglas de acceso para la gestión de quizzes desde el panel de administración.
/// Admin y super admin gestionan cualquier quiz; el instructor solo los de cursos
/// que él creó (misma regla que cursos y lecciones). Cualquier otro rol queda fuera.
/// </summary>
internal static class QuizAuthoringAccess
{
    public const string ForbiddenMessage =
        "Solo el instructor que creó el curso puede gestionar sus quizzes.";

    public static bool CanManage(Course course, Guid requesterId, string requesterRole) =>
        ParseRole(requesterRole) switch
        {
            UserRole.Admin or UserRole.SuperAdmin => true,
            UserRole.Instructor => course.CreatedBy == requesterId,
            _ => false
        };

    public static async Task<bool> CanManageCourseAsync(
        Guid courseId, Guid requesterId, string requesterRole,
        ICourseRepository courses, CancellationToken ct)
    {
        var role = ParseRole(requesterRole);
        if (role is UserRole.Admin or UserRole.SuperAdmin)
            return true;
        if (role is not UserRole.Instructor)
            return false;

        var course = await courses.GetByIdAsync(courseId, ct);
        return course is not null && course.CreatedBy == requesterId;
    }

    public static async Task<bool> CanManageLessonAsync(
        Guid lessonId, Guid requesterId, string requesterRole,
        ICourseRepository courses, ILessonRepository lessons, CancellationToken ct)
    {
        var role = ParseRole(requesterRole);
        if (role is UserRole.Admin or UserRole.SuperAdmin)
            return true;
        if (role is not UserRole.Instructor)
            return false;

        var lesson = await lessons.GetByIdAsync(lessonId, ct);
        return lesson is not null
            && await CanManageCourseAsync(lesson.CourseId, requesterId, requesterRole, courses, ct);
    }

    /// <summary>
    /// Una pregunta pertenece a un curso directamente (examen final) o a través de su lección.
    /// </summary>
    public static Task<bool> CanManageQuestionAsync(
        QuizQuestion question, Guid requesterId, string requesterRole,
        ICourseRepository courses, ILessonRepository lessons, CancellationToken ct)
    {
        if (question.CourseId is { } courseId)
            return CanManageCourseAsync(courseId, requesterId, requesterRole, courses, ct);
        if (question.LessonId is { } lessonId)
            return CanManageLessonAsync(lessonId, requesterId, requesterRole, courses, lessons, ct);

        return Task.FromResult(false);
    }

    /// <summary>
    /// Variante para opciones: solo carga la pregunta cuando hace falta comprobar la propiedad.
    /// </summary>
    public static async Task<bool> CanManageQuestionAsync(
        Guid questionId, Guid requesterId, string requesterRole,
        IQuizRepository quizzes, ICourseRepository courses, ILessonRepository lessons, CancellationToken ct)
    {
        var role = ParseRole(requesterRole);
        if (role is UserRole.Admin or UserRole.SuperAdmin)
            return true;
        if (role is not UserRole.Instructor)
            return false;

        var question = await quizzes.GetQuestionByIdAsync(questionId, ct);
        return question is not null
            && await CanManageQuestionAsync(question, requesterId, requesterRole, courses, lessons, ct);
    }

    // Solo nombres de rol: Enum.TryParse también aceptaría valores numéricos ("1" → Admin).
    private static UserRole? ParseRole(string requesterRole) =>
        Enum.GetValues<UserRole>()
            .Cast<UserRole?>()
            .FirstOrDefault(r => string.Equals(r.ToString(), requesterRole, StringComparison.OrdinalIgnoreCase));
}
