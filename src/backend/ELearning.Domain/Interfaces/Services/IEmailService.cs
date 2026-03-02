namespace ELearning.Domain.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string to, string fullName, string token, CancellationToken ct = default);
    Task SendPasswordResetAsync(string to, string fullName, string token, CancellationToken ct = default);
}
