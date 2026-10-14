using ELearning.Domain.Enums;

namespace ELearning.Domain.Constants;

/// <summary>
/// Nombres de rol tal como viajan en el JWT, en la API y en <c>[Authorize(Roles = ...)]</c>.
/// Son la única fuente de estos textos: no escribas "admin" o "superadmin" a mano.
/// </summary>
public static class RoleNames
{
    public const string Student = "student";
    public const string Instructor = "instructor";
    public const string Admin = "admin";
    public const string SuperAdmin = "superadmin";

    // Combinaciones para [Authorize(Roles = ...)], que acepta una lista separada por comas.
    public const string Admins = Admin + "," + SuperAdmin;
    public const string Staff = Instructor + "," + Admin + "," + SuperAdmin;

    public static IReadOnlyList<string> All { get; } = [Student, Instructor, Admin, SuperAdmin];

    public static string ToRoleName(this UserRole role) => role switch
    {
        UserRole.Student => Student,
        UserRole.Instructor => Instructor,
        UserRole.Admin => Admin,
        UserRole.SuperAdmin => SuperAdmin,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };

    /// <summary>
    /// Solo acepta los nombres de <see cref="All"/>, sin distinguir mayúsculas.
    /// A diferencia de <c>Enum.TryParse</c>, rechaza valores numéricos ("1" → Admin).
    /// </summary>
    public static bool TryParse(string? roleName, out UserRole role)
    {
        foreach (var candidate in Enum.GetValues<UserRole>())
        {
            if (string.Equals(candidate.ToRoleName(), roleName, StringComparison.OrdinalIgnoreCase))
            {
                role = candidate;
                return true;
            }
        }

        role = default;
        return false;
    }
}
