using ELearning.Domain.Constants;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Common.Authorization;

/// <summary>
/// Reglas de autorización sobre cursos, compartidas por los handlers de cursos, lecciones,
/// quizzes, detalle de curso e inscripción. El rol llega como texto del JWT y se interpreta
/// con <see cref="RoleNames.TryParse"/>: un rol desconocido no tiene ningún permiso.
/// </summary>
/// <remarks>
/// <para>
/// <b>Gestionar</b> un curso es editarlo, publicarlo, eliminarlo y administrar sus lecciones y
/// quizzes (<see cref="CanManage"/>):
/// </para>
/// <list type="bullet">
///   <item><b>superadmin</b>: cualquier curso, incluidos los globales.</item>
///   <item><b>admin</b>: los cursos no globales que tienen su país entre sus países
///   (<see cref="Course.CourseCountries"/>).</item>
///   <item><b>instructor</b>: los cursos de los que es el instructor asignado
///   (<see cref="Course.InstructorId"/>), sean globales o no. Haber creado el curso
///   (<see cref="Course.CreatedBy"/>) no da ningún permiso.</item>
/// </list>
/// <para>Además:</para>
/// <list type="bullet">
///   <item>Solo admin y superadmin asignan o quitan el instructor de un curso que gestionan, y
///   solo a un instructor que pueden ver (<see cref="CanAssignInstructor"/>,
///   <see cref="IsEligibleInstructor"/>).</item>
///   <item>Fuera del superadmin, en los países de un curso solo se puede añadir o quitar el propio
///   (<see cref="CanAssignCountries"/>).</item>
///   <item>Solo el superadmin crea cursos globales o cambia si un curso es global. Esta regla la
///   aplican los handlers de crear y editar curso con <see cref="IsSuperAdmin"/>.</item>
///   <item>Quien no gestiona un curso solo lo ve, o se inscribe, si está publicado y disponible
///   en su país (<see cref="IsAvailableIn"/>).</item>
/// </list>
/// </remarks>
public static class CourseAccess
{
    public const string ForbiddenMessage = "No tienes permiso para gestionar este curso.";
    public const string GlobalForbiddenMessage = "Solo el superadmin puede crear cursos globales o cambiar si un curso es global.";
    public const string AssignInstructorForbiddenMessage = "Solo admin y superadmin pueden asignar el instructor de un curso que gestionan.";
    public const string CountriesForbiddenMessage = "Solo puedes añadir o quitar tu propio país en la asignación del curso.";

    public static bool CanManage(Course course, Guid requesterId, string requesterRole, int requesterCountryId) =>
        ParseRole(requesterRole) switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => !course.IsGlobal
                && course.CourseCountries.Any(cc => cc.CountryId == requesterCountryId),
            UserRole.Instructor => course.InstructorId == requesterId,
            _ => false
        };

    /// <summary>Admin y superadmin asignan o quitan el instructor de los cursos que gestionan.</summary>
    public static bool CanAssignInstructor(Course course, Guid requesterId, string requesterRole, int requesterCountryId) =>
        ParseRole(requesterRole) is UserRole.Admin or UserRole.SuperAdmin
        && CanManage(course, requesterId, requesterRole, requesterCountryId);

    /// <summary>
    /// Quién puede quedar asignado: un usuario con rol instructor que el solicitante puede ver
    /// (el admin, los de su país; el superadmin, cualquiera). No se exige que su país sea uno
    /// de los del curso.
    /// </summary>
    public static bool IsEligibleInstructor(User candidate, string requesterRole, int requesterCountryId) =>
        candidate.Role == UserRole.Instructor
        && UserAccess.CanView(candidate, requesterRole, requesterCountryId);

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

    private static UserRole? ParseRole(string requesterRole) =>
        RoleNames.TryParse(requesterRole, out var role) ? role : null;
}
