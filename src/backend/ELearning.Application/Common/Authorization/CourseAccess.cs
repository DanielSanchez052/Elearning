using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Common.Authorization;

/// <summary>
/// Quién puede gestionar un curso (editarlo, publicarlo, borrarlo y gestionar sus lecciones y quizzes):
/// <list type="bullet">
///   <item><b>superadmin</b>: cualquier curso, incluidos los globales.</item>
///   <item><b>admin</b>: cursos no globales asignados a su país.</item>
///   <item><b>instructor</b>: cursos no globales que él creó.</item>
/// </list>
/// Solo el superadmin puede crear cursos globales o marcar un curso como global.
/// </summary>
public static class CourseAccess
{
    public const string ForbiddenMessage = "No tienes permiso para gestionar este curso.";
    public const string GlobalForbiddenMessage = "Solo el superadmin puede crear o marcar cursos como globales.";
    public const string CountriesForbiddenMessage = "Solo puedes añadir o quitar tu propio país en la asignación del curso.";

    public static bool CanManage(Course course, Guid requesterId, string requesterRole, int requesterCountryId) =>
        ParseRole(requesterRole) switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => !course.IsGlobal
                && course.CourseCountries.Any(cc => cc.CountryId == requesterCountryId),
            UserRole.Instructor => !course.IsGlobal && course.CreatedBy == requesterId,
            _ => false
        };

    /// <summary>
    /// Fuera del superadmin, una reasignación de países solo puede añadir o quitar el país
    /// del propio usuario; los demás países del curso deben quedar como estaban.
    /// </summary>
    public static bool CanAssignCountries(
        Course course, IEnumerable<int> newCountryIds, string requesterRole, int requesterCountryId)
    {
        if (ParseRole(requesterRole) == UserRole.SuperAdmin)
            return true;

        var current = course.CourseCountries.Select(cc => cc.CountryId).ToHashSet();
        current.SymmetricExceptWith(newCountryIds);
        return current.All(id => id == requesterCountryId);
    }

    /// <summary>
    /// Disponibilidad para quien no gestiona el curso (misma regla que el catálogo):
    /// publicado y, además, global o asignado a su país.
    /// </summary>
    public static bool IsAvailableIn(Course course, int countryId) =>
        course.IsActive
        && (course.IsGlobal || course.CourseCountries.Any(cc => cc.CountryId == countryId));

    public static bool IsSuperAdmin(string requesterRole) =>
        ParseRole(requesterRole) == UserRole.SuperAdmin;

    // Solo nombres de rol: Enum.TryParse también aceptaría valores numéricos ("1" → Admin).
    public static UserRole? ParseRole(string requesterRole) =>
        Enum.GetValues<UserRole>()
            .Cast<UserRole?>()
            .FirstOrDefault(r => string.Equals(r.ToString(), requesterRole, StringComparison.OrdinalIgnoreCase));
}
