using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Gamification.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Gamification.Queries;

public sealed record GetMyBadgesQuery(Guid UserId) : IQuery<List<UserBadgeDto>>;

public sealed class GetMyBadgesHandler : IQueryHandler<GetMyBadgesQuery, List<UserBadgeDto>>
{
    private readonly IBadgeRepository _badges;

    public GetMyBadgesHandler(IBadgeRepository badges)
    {
        _badges = badges;
    }

    public async Task<Result<List<UserBadgeDto>>> HandleAsync(GetMyBadgesQuery query, CancellationToken ct = default)
    {
        if (query.UserId == Guid.Empty)
            return Result.ValidationFailure<List<UserBadgeDto>>("UserId es requerido");

        var userBadges = await _badges.GetByUserAsync(query.UserId, ct);

        var dtos = userBadges
            .OrderByDescending(ub => ub.ObtainedAt)
            .Select(ub => new UserBadgeDto(
                Id: ub.Id,
                Code: ub.Badge.Code,
                Name: ub.Badge.Name,
                Description: ub.Badge.Description ?? string.Empty,
                ObtainedAt: ub.ObtainedAt,
                CourseId: ub.CourseId,
                CourseTitle: ub.Course?.Title
            ))
            .ToList();

        return dtos;
    }
}
