using ELearning.API.Extensions;
using ELearning.API.Models;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Users.Commands;
using ELearning.Application.Features.Users.DTOs;
using ELearning.Application.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin,superadmin")]
public class AdminUsersController(
    IQueryHandler<GetUsersQuery, PagedResult<UserSummaryDto>> getUsersHandler,
    IQueryHandler<GetUserByIdQuery, UserSummaryDto> getUserByIdHandler,
    ICommandHandler<ChangeUserRoleCommand> changeRoleHandler,
    ICommandHandler<ChangeUserCountryCommand> changeCountryHandler
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int? countryId = null,
        [FromQuery] string? role = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isEmailVerified = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var effectiveCountryId = User.IsAdmin() && !User.IsSuperAdmin()
            ? User.GetCountryId()
            : countryId;

        var query = new GetUsersQuery(
            CountryId: effectiveCountryId,
            Role: role,
            Search: search,
            IsEmailVerified: isEmailVerified,
            Page: page,
            PageSize: pageSize
        );

        var result = await getUsersHandler.HandleAsync(query, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await getUserByIdHandler.HandleAsync(
            new GetUserByIdQuery(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleRequest request)
    {
        var cmd = new ChangeUserRoleCommand(
            TargetUserId: id,
            NewRole: request.Role,
            RequesterId: User.GetUserId(),
            RequesterRole: User.GetRole()
        );

        var result = await changeRoleHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}/country")]
    [Authorize(Roles = "superadmin")]
    public async Task<IActionResult> ChangeUserCountry(Guid id, [FromBody] ChangeCountryRequest request)
    {
        var cmd = new ChangeUserCountryCommand(
            TargetUserId: id,
            NewCountryId: request.CountryId
        );

        var result = await changeCountryHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }
}
