using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ELearning.API.Extensions;

public static class CorsExtensions
{
    public const string AllowAllPolicy = "AllowAll";
    public const string FrontendPolicy = "Frontend";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AllowAllPolicy, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            options.AddPolicy(FrontendPolicy, policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseCorsConfiguration(this WebApplication app)
    {
        app.UseCors(AllowAllPolicy);
        return app;
    }
}
