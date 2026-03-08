using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Users.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserSummaryDto>;

public sealed class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserSummaryDto>
{
    private readonly IUserRepository _users;

    public GetUserByIdHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<UserSummaryDto>> HandleAsync(GetUserByIdQuery query, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Result.NotFound<UserSummaryDto>($"Usuario con id '{query.UserId}' no encontrado.");

        return Result.Success(new UserSummaryDto(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            Role: user.Role.ToString().ToLowerInvariant(),
            Country: user.Country.Name,
            CountryId: user.CountryId,
            IsEmailVerified: user.IsEmailVerified,
            CreatedAt: user.CreatedAt
        ));
    }
}
