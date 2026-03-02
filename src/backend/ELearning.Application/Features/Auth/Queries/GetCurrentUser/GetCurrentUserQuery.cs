using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Auth.DTOs.User;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(
    Guid UserId
) : IQuery<UserDto>;

public sealed class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _users;

    public GetCurrentUserHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<UserDto>> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);

        if (user is null)
            return Result.NotFound<UserDto>(
                $"Usuario con id '{query.UserId}' no encontrado.");

        var dto = new UserDto(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            Role: user.Role.ToString().ToLowerInvariant(),
            Country: user.Country.Name,
            CountryId: user.CountryId,
            CreatedAt: user.CreatedAt,
            LoginStreak: user.LoginStreak
        );

        return Result.Success(dto);
    }
}