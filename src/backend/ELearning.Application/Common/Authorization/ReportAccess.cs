using ELearning.Domain.Constants;
using ELearning.Domain.Enums;

namespace ELearning.Application.Common.Authorization;

/// <summary>
/// Quién ve cada reporte y sobre qué país:
/// <list type="bullet">
///   <item><b>Reportes de la plataforma</b> (resumen, usuarios, rankings): admin y superadmin.
///   El superadmin elige país o ve todos; el admin, siempre el suyo.</item>
///   <item><b>Reportes por curso</b>: además, el instructor, sobre los cursos que tiene asignados
///   (ver <see cref="CourseAccess.CanManage"/>).</item>
/// </list>
/// </summary>
public static class ReportAccess
{
    public const string ForbiddenMessage = "Solo admin y superadmin pueden ver los reportes de la plataforma.";

    public static bool CanViewPlatformReports(string requesterRole) =>
        ParseRole(requesterRole) is UserRole.Admin or UserRole.SuperAdmin;

    /// <summary>
    /// País sobre el que se calcula el reporte: el que pida el superadmin (null = todos) o, para
    /// el admin, siempre el suyo, aunque pida otro.
    /// </summary>
    public static int? ResolveCountry(string requesterRole, int requesterCountryId, int? requestedCountryId) =>
        ParseRole(requesterRole) == UserRole.SuperAdmin ? requestedCountryId : requesterCountryId;

    private static UserRole? ParseRole(string requesterRole) =>
        RoleNames.TryParse(requesterRole, out var role) ? role : null;
}
