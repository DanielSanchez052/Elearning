using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;

namespace ELearning.Application.Features.Auth.Commands;

public sealed record ForgotPasswordCommand(
    string Email
) : ICommand;

public sealed class ForgotPasswordHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _users;
    private readonly IEmailService _email;

    public ForgotPasswordHandler(IUserRepository users, IEmailService email)
    {
        _users = users;
        _email = email;
    }

    public async Task<Result> HandleAsync(ForgotPasswordCommand cmd, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailTrackedAsync(cmd.Email, ct);
        
        if (user is null)
            return Result.Success();

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        user.SetResetToken(token, expiresAt);
        await _users.UpdateAsync(user, ct);

        _ = _email.SendPasswordResetAsync(
            to: user.Email,
            fullName: user.FullName,
            token: token,
            ct: CancellationToken.None
        );

        return Result.Success();
    }
}