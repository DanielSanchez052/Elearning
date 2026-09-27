using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// Always proposes LoginFirst. It does not check whether this is really the
/// user's first login — BadgeAwardService's HasBadgeAsync check upstream is
/// what makes repeat logins a no-op, so the rule itself stays trivial and
/// side-effect free.
/// </summary>
public sealed class FirstLoginRule : ILoginBadgeRule
{
    public IReadOnlyList<BadgeAwardProposal> Evaluate(User user) =>
        new[] { new BadgeAwardProposal(BadgeCode.LoginFirst) };
}
