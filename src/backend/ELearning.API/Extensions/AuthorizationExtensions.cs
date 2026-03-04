using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ELearning.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            //// Política para solo administradores
            //options.AddPolicy("AdminOnly", policy =>
            //    policy.RequireRole("Admin"));

            //// Política para profesores e instructores
            //options.AddPolicy("TeacherOrAdmin", policy =>
            //    policy.RequireRole("Teacher", "Instructor", "Admin"));

            //// Política para usuarios con email verificado
            //options.AddPolicy("VerifiedEmail", policy =>
            //    policy.RequireClaim("email_verified", "true"));

            //// Política para contenido premium (requiere suscripción)
            //options.AddPolicy("PremiumUser", policy =>
            //    policy.RequireClaim("subscription_level", "premium", "pro"));

            //// Política combinada: admin o teacher con email verificado
            //options.AddPolicy("TeacherVerified", policy =>
            //    policy.RequireRole("Teacher", "Admin")
            //          .RequireClaim("email_verified", "true"));
        });

        return services;
    }
}
