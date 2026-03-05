using ELearning.API.Extensions;
using ELearning.API.Models;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Admin.Commands;
using ELearning.Application.Features.Admin.DTOs;
using ELearning.Application.Features.Admin.Queries;
using ELearning.Application.Features.Countries.Dtos;
using ELearning.Application.Features.Countries.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize] // todos los endpoints de este controller requieren autenticación
public class AdminController(
    // Countries
    ICommandHandler<CreateCountryCommand, int> createCountryHandler,
    ICommandHandler<ToggleCountryStatusCommand> toggleCountryHandler,
    IQueryHandler<GetCountriesQuery, IReadOnlyList<CountryDto>> getCountriesHandler,
    IQueryHandler<GetCountryByIdQuery, CountryDto> getCountryByIdHandler,
    // Users
    IQueryHandler<GetUsersQuery, PagedResult<UserSummaryDto>> getUsersHandler,
    IQueryHandler<GetUserByIdQuery, UserSummaryDto> getUserByIdHandler,
    ICommandHandler<ChangeUserRoleCommand> changeRoleHandler,
    ICommandHandler<ChangeUserCountryCommand> changeCountryHandler
) : ControllerBase
{
    // ── COUNTRIES ─────────────────────────────────────────────────────────────

    /// GET api/admin/countries
    [HttpGet("countries")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> GetCountries()
    {
        var result = await getCountriesHandler.HandleAsync(
            new GetCountriesQuery(),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// GET api/admin/countries/{id}
    [HttpGet("countries/{id:int}")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> GetCountryById(int id)
    {
        var result = await getCountryByIdHandler.HandleAsync(
            new GetCountryByIdQuery(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// POST api/admin/countries
    [HttpPost("countries")]
    [Authorize(Roles = "superadmin")]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryCommand cmd)
    {
        var result = await createCountryHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToCreatedResult(result, nameof(GetCountryById), new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// PATCH api/admin/countries/{id}/toggle-status
    [HttpPatch("countries/{id:int}/toggle-status")]
    [Authorize(Roles = "superadmin")]
    public async Task<IActionResult> ToggleCountryStatus(int id)
    {
        var result = await toggleCountryHandler.HandleAsync(
            new ToggleCountryStatusCommand(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    // ── USERS ─────────────────────────────────────────────────────────────────

    /// GET api/admin/users?countryId=1&role=student&search=juan&isEmailVerified=true&page=1&pageSize=20
    /// Admin ve solo su país — Super Admin puede ver todos
    [HttpGet("users")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int? countryId = null,
        [FromQuery] string? role = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isEmailVerified = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Si es Admin (no Super Admin), forzar el filtro al propio país
        // independientemente de lo que mande en el query string
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

    /// GET api/admin/users/{id}
    [HttpGet("users/{id:guid}")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await getUserByIdHandler.HandleAsync(
            new GetUserByIdQuery(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    /// PATCH api/admin/users/{id}/role
    [HttpPatch("users/{id:guid}/role")]
    [Authorize(Roles = "admin,superadmin")]
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

    /// PATCH api/admin/users/{id}/country
    [HttpPatch("users/{id:guid}/country")]
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

