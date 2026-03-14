using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.DTOs.AuthResponse;
using ELearning.Application.Features.Auth.DTOs.User;
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

    public LoginHandler(
        IUserRepository users,
        IPasswordHasherService hasher,
        IJwtService jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
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

        return Result.Success(response);
    }
}