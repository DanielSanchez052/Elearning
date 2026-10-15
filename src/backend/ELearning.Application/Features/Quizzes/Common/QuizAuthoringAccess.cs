using ELearning.Application.Common.Authorization;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Common;

/// <summary>
/// Acceso a la gestión de quizzes desde el panel de administración: se puede gestionar
/// un quiz si se puede gestionar su curso (ver <see cref="CourseAccess"/>). Resuelve el
/// curso a partir de la lección o la pregunta, y evita las consultas para el superadmin.
/// </summary>
internal static class QuizAuthoringAccess
{
    public const string ForbiddenMessage =
        "No tienes permiso para gestionar los quizzes de este curso.";

    public static bool CanManage(Course course, Guid requesterId, string requesterRole, int requesterCountryId) =>
        CourseAccess.CanManage(course, requesterId, requesterRole, requesterCountryId);

    public static async Task<bool> CanManageCourseAsync(
        Guid courseId, Guid requesterId, string requesterRole, int requesterCountryId,
        ICourseRepository courses, CancellationToken ct)
    {
        if (CourseAccess.IsSuperAdmin(requesterRole))
            return true;

        var course = await courses.GetByIdAsync(courseId, ct);
        return course is not null
            && CourseAccess.CanManage(course, requesterId, requesterRole, requesterCountryId);
    }

    public static async Task<bool> CanManageLessonAsync(
        Guid lessonId, Guid requesterId, string requesterRole, int requesterCountryId,
        ICourseRepository courses, ILessonRepository lessons, CancellationToken ct)
    {
        if (CourseAccess.IsSuperAdmin(requesterRole))
            return true;

        var lesson = await lessons.GetByIdAsync(lessonId, ct);
        return lesson is not null
            && await CanManageCourseAsync(lesson.CourseId, requesterId, requesterRole, requesterCountryId, courses, ct);
    }

    /// <summary>
    /// Una pregunta pertenece a un curso directamente (examen final) o a través de su lección.
    /// </summary>
    public static Task<bool> CanManageQuestionAsync(
        QuizQuestion question, Guid requesterId, string requesterRole, int requesterCountryId,
        ICourseRepository courses, ILessonRepository lessons, CancellationToken ct)
    {
        if (question.CourseId is { } courseId)
            return CanManageCourseAsync(courseId, requesterId, requesterRole, requesterCountryId, courses, ct);
        if (question.LessonId is { } lessonId)
            return CanManageLessonAsync(lessonId, requesterId, requesterRole, requesterCountryId, courses, lessons, ct);

        return Task.FromResult(false);
    }

    /// <summary>
    /// Variante para opciones: solo carga la pregunta cuando hace falta comprobar el acceso.
    /// </summary>
    public static async Task<bool> CanManageQuestionAsync(
        Guid questionId, Guid requesterId, string requesterRole, int requesterCountryId,
        IQuizRepository quizzes, ICourseRepository courses, ILessonRepository lessons, CancellationToken ct)
    {
        if (CourseAccess.IsSuperAdmin(requesterRole))
            return true;

        var question = await quizzes.GetQuestionByIdAsync(questionId, ct);
        return question is not null
            && await CanManageQuestionAsync(question, requesterId, requesterRole, requesterCountryId, courses, lessons, ct);
    }
}
