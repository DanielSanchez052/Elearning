using ELearning.Domain.Interfaces.Services;

namespace ELearning.Infrastructure.Services;

public class EmailService : IEmailService
{
    public Task SendEmailVerificationAsync(string to, string fullName, string token, CancellationToken ct = default)
    {
        Console.WriteLine("=== Email de Verificación ===");
        Console.WriteLine($"Para: {to}");
        Console.WriteLine($"Nombre: {fullName}");
        Console.WriteLine($"Token: {token}");
        Console.WriteLine("=============================");

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string to, string fullName, string token, CancellationToken ct = default)
    {
        Console.WriteLine("=== Email de Reseteo de contraseña ===");
        Console.WriteLine($"Para: {to}");
        Console.WriteLine($"Nombre: {fullName}");
        Console.WriteLine($"Token: {token}");
        Console.WriteLine("=============================");

        return Task.CompletedTask;
    }
}
