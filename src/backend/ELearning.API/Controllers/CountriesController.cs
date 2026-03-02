using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Dtos;
using ELearning.Application.Features.Countries.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

// TODO: Si en el futuro se necesitan endpoints públicos para estados/provincias, considerar crear un LocationController con endpoints anidados (e.g. GET api/locations/countries/{countryId}/states) para mantener la jerarquía geográfica clara.

/// <summary>
/// Endpoint público de países — no requiere autenticación.
/// Usado principalmente para poblar el dropdown de registro.
/// Para gestión de países (crear, activar/desactivar) ver AdminController.
/// </summary>
[ApiController]
[Route("api/countries")]
public class CountriesController(
    IQueryHandler<GetCountriesQuery, IReadOnlyList<CountryDto>> getCountriesHandler
) : ControllerBase
{
    // GET api/countries
    // Devuelve solo países activos — sin token requerido
    [HttpGet]
    public async Task<IActionResult> GetActiveCountries()
    {
        var result = await getCountriesHandler.HandleAsync(
            new GetCountriesQuery(OnlyActive: true),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }
}