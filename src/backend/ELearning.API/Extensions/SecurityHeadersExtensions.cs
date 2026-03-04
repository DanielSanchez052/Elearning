using Microsoft.AspNetCore.Builder;

namespace ELearning.API.Extensions;

public static class SecurityHeadersExtensions
{
    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            // Prevenir ataques de tipo Content-Type sniffing
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // Prevenir clickjacking
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // Protección contra XSS (Cross-Site Scripting)
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Política de referrer
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content Security Policy - Ajustar según necesidad
            context.Response.Headers.Add(
                "Content-Security-Policy",
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'"
            );

            // Permiso de características del navegador
            context.Response.Headers.Add(
                "Permissions-Policy",
                "geolocation=(), microphone=(), camera=()"
            );

            await next();
        });

        return app;
    }
}
