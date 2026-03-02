using ELearning.Application.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Extensions;

/// <summary>
/// Convierte un Result en IActionResult sin repetir el switch en cada controlador.
/// </summary>
public static class ControllerExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
            return controller.Ok(result.Value);

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => controller.NotFound(new { error = result.Error }),
            ResultErrorType.Conflict => controller.Conflict(new { error = result.Error }),
            ResultErrorType.Forbidden => controller.Forbid(),
            ResultErrorType.Unauthorized => controller.Unauthorized(new { error = result.Error }),
            ResultErrorType.Validation => controller.BadRequest(new { error = result.Error }),
            _ => controller.StatusCode(500, new { error = result.Error })
        };
    }

    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
            return controller.NoContent();

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => controller.NotFound(new { error = result.Error }),
            ResultErrorType.Conflict => controller.Conflict(new { error = result.Error }),
            ResultErrorType.Forbidden => controller.Forbid(),
            ResultErrorType.Unauthorized => controller.Unauthorized(new { error = result.Error }),
            ResultErrorType.Validation => controller.BadRequest(new { error = result.Error }),
            _ => controller.StatusCode(500, new { error = result.Error })
        };
    }

    // Variante para cuando el resultado exitoso debe ser 201 Created
    public static IActionResult ToCreatedResult<T>(
        this ControllerBase controller,
        Result<T> result,
        string actionName,
        object routeValues)
    {
        if (result.IsSuccess)
            return controller.CreatedAtAction(actionName, routeValues, result.Value);

        return controller.ToActionResult(result);
    }
}