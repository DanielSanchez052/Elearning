using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ELearning.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        return services;
    }

    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        return app;
    }
}
