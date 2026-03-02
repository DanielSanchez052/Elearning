using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;

namespace ELearning.Application.Features.Auth.Commands;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword
) : ICommand;

public sealed class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasherService _hasher;

    public ResetPasswordHandler(IUserRepository users, IPasswordHasherService hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<Result> HandleAsync(ResetPasswordCommand cmd, CancellationToken ct = default)
    {
        var user = await _users.GetByResetTokenTrackedAsync(cmd.Token, ct);

        if (user is null)
            return Result.NotFound("El token de recuperación no es válido.");

        if (user.ResetTokenExpires is null || user.ResetTokenExpires < DateTime.UtcNow)
            return Result.Conflict("El token de recuperación ha expirado. Solicita uno nuevo.");

        var newHash = _hasher.Hash(cmd.NewPassword);
        user.SetPasswordHash(newHash);

        user.ClearResetToken();

        await _users.UpdateAsync(user, ct);

        return Result.Success();
    }
}
