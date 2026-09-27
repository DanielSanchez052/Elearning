using ELearning.Application.Features.Gamification.DTOs;
using ELearning.Application.Features.Gamification.Rules;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Gamification.Services;

public sealed class BadgeAwardService : IBadgeAwardService
{
    private readonly IBadgeRepository _badges;
    private readonly IReadOnlyList<ILoginBadgeRule> _loginRules;
    private readonly IReadOnlyList<ICourseCompletionBadgeRule> _courseCompletionRules;

    public BadgeAwardService(
        IBadgeRepository badges,
        IEnumerable<ILoginBadgeRule> loginRules,
        IEnumerable<ICourseCompletionBadgeRule> courseCompletionRules)
    {
        _badges = badges;
        _loginRules = loginRules.ToList();
        _courseCompletionRules = courseCompletionRules.ToList();
    }

    public Task<IReadOnlyList<AwardedBadgeDto>> OnUserLoggedInAsync(User user, CancellationToken ct = default)
    {
        var proposals = _loginRules.SelectMany(rule => rule.Evaluate(user));
        return AwardAsync(user.Id, proposals, ct);
    }

    public Task<IReadOnlyList<AwardedBadgeDto>> OnCourseCompletedAsync(CourseEnrollment enrollment, CancellationToken ct = default)
    {
        var proposals = _courseCompletionRules.SelectMany(rule => rule.Evaluate(enrollment));
        return AwardAsync(enrollment.UserId, proposals, ct);
    }

    public Task<IReadOnlyList<AwardedBadgeDto>> OnCourseExamPassedAsync(UserQuizResult result, CancellationToken ct = default)
    {
        // TODO(Track A): wire SpeedsterRule once ExamSession/Duration land.
        return Task.FromResult<IReadOnlyList<AwardedBadgeDto>>(Array.Empty<AwardedBadgeDto>());
    }

    private async Task<IReadOnlyList<AwardedBadgeDto>> AwardAsync(
        Guid userId, IEnumerable<BadgeAwardProposal> proposals, CancellationToken ct)
    {
        var awarded = new List<AwardedBadgeDto>();

        foreach (var proposal in proposals)
        {
            var badge = await _badges.GetByCodeAsync(proposal.Code, ct);
            if (badge is null)
                continue; // badge code not seeded yet — nothing to award

            var alreadyAwarded = await _badges.HasBadgeAsync(userId, badge.Id, proposal.CourseId, ct);
            if (alreadyAwarded)
                continue; // fast path: idempotent, no insert attempted

            var userBadge = UserBadge.Create(userId, badge.Id, proposal.CourseId);
            var inserted = await _badges.TryAddAsync(userBadge, ct);
            if (!inserted)
                continue; // race safety net: someone else awarded it first

            awarded.Add(new AwardedBadgeDto(badge.Id, badge.Code, badge.Name, proposal.CourseId, userBadge.ObtainedAt));
        }

        return awarded;
    }
}
