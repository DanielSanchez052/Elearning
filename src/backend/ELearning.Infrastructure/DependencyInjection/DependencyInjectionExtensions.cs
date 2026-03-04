using Microsoft.Extensions.DependencyInjection;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Repositories;
using ELearning.Infrastructure.Services;

namespace ELearning.Infrastructure.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>();

        services.AddRepositories();

        services.AddServices();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IBadgeRepository, BadgeRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtService, JwtService>();
        return services;
    }
}
