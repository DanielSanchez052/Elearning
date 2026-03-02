using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Reports.DTOs;

namespace ELearning.Application.Features.Reports.Queries;

public class GetDashboardQuery : IQuery<DashboardDto> { }
public class GetDashboardHandler : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    Task<Result<DashboardDto>> IQueryHandler<GetDashboardQuery, DashboardDto>.HandleAsync(GetDashboardQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class GetLeaderboardQuery : IQuery<List<LeaderboardDto>> { public int CountryId { get; set; } }
public class GetLeaderboardHandler : IQueryHandler<GetLeaderboardQuery, List<LeaderboardDto>>
{
    Task<Result<List<LeaderboardDto>>> IQueryHandler<GetLeaderboardQuery, List<LeaderboardDto>>.HandleAsync(GetLeaderboardQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
