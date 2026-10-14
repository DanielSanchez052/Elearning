using Microsoft.EntityFrameworkCore;
using ELearning.Infrastructure.Persistence;

namespace ELearning.API.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddDbContextConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // The connection string carries a password, so it must not be committed
        // in appsettings.json. Fail fast with the exact way to configure it.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. Set it with: " +
                "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<connection string>\" --project ELearning.API " +
                "(or the ConnectionStrings__DefaultConnection environment variable).");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
            });
            
            if (configuration.GetValue<bool>("EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}
