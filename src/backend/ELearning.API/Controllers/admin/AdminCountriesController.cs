using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Commands;
using ELearning.Application.Features.Countries.Dtos;
using ELearning.Application.Features.Countries.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/admin/countries")]
[Authorize(Roles = "admin,superadmin")]
public class AdminCountriesController(
    ICommandHandler<CreateCountryCommand, int> createCountryHandler,
    ICommandHandler<ToggleCountryStatusCommand> toggleCountryHandler,
    IQueryHandler<GetCountriesQuery, IReadOnlyList<CountryDto>> getCountriesHandler,
    IQueryHandler<GetCountryByIdQuery, CountryDto> getCountryByIdHandler
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCountries()
    {
        var result = await getCountriesHandler.HandleAsync(
            new GetCountriesQuery(),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCountryById(int id)
    {
        var result = await getCountryByIdHandler.HandleAsync(
            new GetCountryByIdQuery(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "superadmin")]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryCommand cmd)
    {
        var result = await createCountryHandler.HandleAsync(cmd, HttpContext.RequestAborted);
        return this.ToCreatedResult(result, nameof(GetCountryById), new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPatch("{id:int}/toggle-status")]
    [Authorize(Roles = "superadmin")]
    public async Task<IActionResult> ToggleCountryStatus(int id)
    {
        var result = await toggleCountryHandler.HandleAsync(
            new ToggleCountryStatusCommand(id),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }
}
