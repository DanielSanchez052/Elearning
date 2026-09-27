using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Gamification.DTOs;
using ELearning.Application.Features.Gamification.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BadgesController(
    IQueryHandler<GetMyBadgesQuery, List<UserBadgeDto>> getMyBadgesHandler
) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBadges(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await getMyBadgesHandler.HandleAsync(new GetMyBadgesQuery(userId), ct);
        return this.ToActionResult(result);
    }
}
