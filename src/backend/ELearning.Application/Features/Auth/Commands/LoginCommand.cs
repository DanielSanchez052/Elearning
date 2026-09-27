using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.DTOs.AuthResponse;
using ELearning.Application.Features.Auth.DTOs.User;
using ELearning.Application.Features.Gamification.Services;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;

namespace ELearning.Application.Features.Auth.Commands;

public sealed record LoginCommand(
    string Email,
    string Password
) : ICommand<LoginResponseDto>;

public sealed class LoginHandler : ICommandHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasherService _hasher;
    private readonly IJwtService _jwt;
    private readonly IBadgeAwardService _badges;

    public LoginHandler(
        IUserRepository users,
        IPasswordHasherService hasher,
        IJwtService jwt,
        IBadgeAwardService badges)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
        _badges = badges;
    }

    public async Task<Result<LoginResponseDto>> HandleAsync(
        LoginCommand cmd,
        CancellationToken ct = default)
    {
        var user = await _users.GetByEmailTrackedAsync(cmd.Email, ct);

        if (user is null || !_hasher.Verify(user.PasswordHash, cmd.Password))
            return Result.Unauthorized<LoginResponseDto>("Email o contraseña incorrectos.");

        if (!user.IsEmailVerified)
            return Result.Unauthorized<LoginResponseDto>(
                "Debes verificar tu email antes de iniciar sesión.");

        user!.RecordLogin();
        await _users.UpdateAsync(user, ct);

        var (token, expiresAt) = _jwt.GenerateAccessToken(user);

        var response = new LoginResponseDto(
            AccessToken: token,
            ExpiresAt: expiresAt,
            User: new LoggedUserDto(
                Id: user.Id,
                FullName: user.FullName,
                Email: user.Email,
                Role: user.Role.ToString().ToLowerInvariant(),
                Country: user.Country?.Name ?? string.Empty
            )
        );

        // Best-effort, after the login was saved: a badge failure never fails the login.
        // Bounded so a slow badge store can't add unbounded latency to every login.
        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
            await _badges.OnUserLoggedInAsync(user, linkedCts.Token);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            // IBadgeAwardService logs its own failures; this only guards the login result.
            // A pure timeout trip (ct itself not cancelled) is swallowed here too - best-effort.
            // Real client cancellation (ct cancelled) is allowed to propagate, per contract.
        }

        return Result.Success(response);
    }
}