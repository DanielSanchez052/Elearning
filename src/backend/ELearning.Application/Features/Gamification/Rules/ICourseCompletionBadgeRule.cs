using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// Evaluated by BadgeAwardService.OnCourseCompletedAsync. Pure function: takes
/// the completed enrollment, proposes zero or more badges scoped to that
/// course. Does not check whether the badge was already awarded — that's the
/// service's job.
/// </summary>
public interface ICourseCompletionBadgeRule
{
    IReadOnlyList<BadgeAwardProposal> Evaluate(CourseEnrollment enrollment);
}
