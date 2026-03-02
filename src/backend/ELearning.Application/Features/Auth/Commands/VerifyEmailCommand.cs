using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Auth.Commands;

public sealed record VerifyEmailCommand(
    string Token
) : ICommand;

public sealed class VerifyEmailHandler : ICommandHandler<VerifyEmailCommand>
{
    private readonly IUserRepository _users;

    public VerifyEmailHandler(IUserRepository users) => _users = users;

    public async Task<Result> HandleAsync(VerifyEmailCommand cmd, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailVerifyTokenTrackedAsync(cmd.Token, ct);

        if (user is null)
            return Result.NotFound("El token de verificación no es válido.");

        if (user.IsEmailVerified)
            return Result.Conflict("El email ya fue verificado anteriormente.");

        user.VerifyEmail();

        await _users.UpdateAsync(user, ct);

        return Result.Success();
    }
}

