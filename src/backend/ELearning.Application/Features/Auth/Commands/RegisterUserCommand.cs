using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Validators;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;

namespace ELearning.Application.Features.Auth.Commands;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    int CountryId
) : ICommand<Guid>;

public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _users;
    private readonly ICountryRepository _countries;
    private readonly IPasswordHasherService _hasher;
    private readonly IEmailService _email;

    public RegisterUserHandler(IUserRepository users, ICountryRepository countries, IPasswordHasherService hasher, IEmailService email)
    {
        _users = users;
        _countries = countries;
        _hasher = hasher;
        _email = email;
    }

    public async Task<Result<Guid>> HandleAsync(RegisterUserCommand cmd, CancellationToken ct = default)
    {
        var country = await _countries.GetByIdAsync(cmd.CountryId, ct);
        if (country is null)
            return Result.NotFound<Guid>($"El país con id '{cmd.CountryId}' no existe.");

        var emailTaken = await _users.ExistsByEmailAsync(cmd.Email, ct);
        if (emailTaken)
            return Result.Conflict<Guid>($"El email '{cmd.Email}' ya está registrado.");

        var passwordHash = _hasher.Hash(cmd.Password);

        var user = User.Create(
            fullName: cmd.FullName,
            email: cmd.Email.ToLowerInvariant(), // normalizar antes de persistir
            passwordHash: passwordHash,
            countryId: cmd.CountryId
        );

        var verifyToken = Guid.NewGuid().ToString("N"); // 32 chars hex, sin guiones
        user.SetEmailVerifyToken(verifyToken);

        await _users.CreateAsync(user, ct);

       
        _ = _email.SendEmailVerificationAsync(
            to: user.Email,
            fullName: user.FullName,
            token: verifyToken,
            ct: CancellationToken.None // no cancelar si el request se cancela
        );

        return Result.Success(user.Id);
    }
}