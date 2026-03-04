using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Admin.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Admin.Queries;


// ══════════════════════════════════════════════════════════════════════════════
// GET USERS (paginado con filtros)
// ══════════════════════════════════════════════════════════════════════════════

public sealed record GetUsersQuery(
    int? CountryId,        // null = todos los países (solo Super Admin lo usa así)
    string? Role,            // null = todos los roles
    string? Search,          // busca en nombre y email
    bool? IsEmailVerified,  // null = todos
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<UserSummaryDto>>;

public sealed class GetUsersHandler
    : IQueryHandler<GetUsersQuery, PagedResult<UserSummaryDto>>
{
    private readonly IUserRepository _users;

    public GetUsersHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<PagedResult<UserSummaryDto>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken ct = default)
    {
        // Clamp de page y pageSize para evitar valores absurdos
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (users, totalCount) = await _users.GetPagedAsync(
            countryId: query.CountryId,
            role: query.Role?.ToLower(),
            search: query.Search,
            isEmailVerified: query.IsEmailVerified,
            page: page,
            pageSize: pageSize,
            ct: ct
        );

        var dtos = users.Select(u => new UserSummaryDto(
            Id: u.Id,
            FullName: u.FullName,
            Email: u.Email,
            Role: u.Role.ToString().ToLowerInvariant(),
            Country: u.Country.Name,
            CountryId: u.CountryId,
            IsEmailVerified: u.IsEmailVerified,
            CreatedAt: u.CreatedAt
        )).ToList().AsReadOnly();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result.Success(new PagedResult<UserSummaryDto>(
            Items: dtos,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        ));
    }
}