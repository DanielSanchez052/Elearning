using ELearning.Domain.Constants;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Common.Authorization;

/// <summary>
/// Gestión de usuarios desde el panel de administración:
/// <list type="bullet">
///   <item><b>superadmin</b>: ve a todos y cambia el rol de cualquiera, incluidos otros superadmins.</item>
///   <item><b>admin</b>: solo ve usuarios de su país, y solo cambia el rol de estudiantes e
///   instructores, a estudiante o instructor. Los admins y superadmins los gestiona el superadmin.</item>
/// </list>
/// Nadie cambia su propio rol: así siempre queda al menos un superadmin.
/// </summary>
public static class UserAccess
{
    public const string ManageStaffForbiddenMessage =
        "Solo el superadmin puede cambiar el rol de un admin o superadmin.";
    public const string AssignRoleForbiddenMessage =
        "Un admin solo puede asignar los roles estudiante e instructor.";
    public const string OwnRoleForbiddenMessage = "No puedes cambiar tu propio rol.";

    /// <summary>Si no puede verlo, se responde 404 para no revelar usuarios de otros países.</summary>
    public static bool CanView(User target, string requesterRole, int requesterCountryId) =>
        ParseRole(requesterRole) switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => target.CountryId == requesterCountryId,
            _ => false
        };

    public static bool CanAssignRole(string requesterRole, UserRole newRole) =>
        ParseRole(requesterRole) switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => newRole is UserRole.Student or UserRole.Instructor,
            _ => false
        };

    /// <summary>Si el rol actual del usuario permite que el solicitante se lo cambie.</summary>
    public static bool CanChangeRoleOf(User target, string requesterRole) =>
        ParseRole(requesterRole) switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => target.Role is UserRole.Student or UserRole.Instructor,
            _ => false
        };

    private static UserRole? ParseRole(string requesterRole) =>
        RoleNames.TryParse(requesterRole, out var role) ? role : null;
}
