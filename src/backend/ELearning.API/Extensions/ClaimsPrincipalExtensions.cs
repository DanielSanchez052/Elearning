using System.Security.Claims;

namespace ELearning.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    private const string SubClaim       = "sub";
    private const string EmailClaim     = "email";
    private const string CountryIdClaim = "country_id";

    /// <summary>
    /// Extrae el Id del usuario desde el claim 'sub' del JWT.
    /// Úsalo en endpoints con [Authorize] — el claim siempre existe si el token es válido.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(SubClaim)
                 ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (value is null)
            throw new InvalidOperationException(
                "El claim 'sub' no está presente en el token. " +
                "Verifica que el endpoint tenga [Authorize].");

        return Guid.Parse(value);
    }

    /// <summary>
    /// Extrae el CountryId del usuario desde el claim 'country_id' del JWT.
    /// Úsalo en handlers de cursos y reportes para filtrar por país
    /// sin hacer un round-trip adicional a la base de datos.
    /// </summary>
    public static int GetCountryId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(CountryIdClaim);

        if (value is null)
            throw new InvalidOperationException(
                "El claim 'country_id' no está presente en el token.");

        return int.Parse(value);
    }

    /// <summary>
    /// Extrae el rol del usuario desde el claim 'role' del JWT.
    /// </summary>
    public static string GetRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role);

        if (value is null)
            throw new InvalidOperationException(
                "El claim 'role' no está presente en el token.");

        return value;
    }

    /// <summary>
    /// Extrae el email del usuario desde el claim 'email' del JWT.
    /// </summary>
    public static string GetEmail(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(EmailClaim)
                 ?? user.FindFirstValue(ClaimTypes.Email);

        if (value is null)
            throw new InvalidOperationException(
                "El claim 'email' no está presente en el token.");

        return value;
    }

    // ── Helpers de rol ────────────────────────────────────────────────────────
    // Evitan hacer string comparisons dispersas por todo el código del controlador.

    public static bool IsSuperAdmin(this ClaimsPrincipal user) =>
        user.IsInRole("super_admin");

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole("admin") || user.IsInRole("super_admin");

    public static bool IsInstructor(this ClaimsPrincipal user) =>
        user.IsInRole("instructor");

    public static bool IsStudent(this ClaimsPrincipal user) =>
        user.IsInRole("student");

    /// <summary>
    /// Verifica si el usuario tiene acceso administrativo
    /// (admin o super_admin).
    /// </summary>
    public static bool HasAdminAccess(this ClaimsPrincipal user) =>
        user.IsAdmin();
}