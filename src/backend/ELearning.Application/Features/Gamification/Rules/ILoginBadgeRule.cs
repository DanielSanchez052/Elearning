using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// Evaluated by BadgeAwardService.OnUserLoggedInAsync. Pure function: takes
/// the login context, proposes zero or more badges. Does not check whether
/// the badge was already awarded — that's the service's job.
/// </summary>
public interface ILoginBadgeRule
{
    IReadOnlyList<BadgeAwardProposal> Evaluate(User user);
}
