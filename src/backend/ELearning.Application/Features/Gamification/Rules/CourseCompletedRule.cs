using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// Proposes CourseDone, scoped to the completed enrollment's course. Once per
/// course (enforced by the service's HasBadgeAsync check against the
/// (UserId, BadgeId, CourseId) unique index).
/// </summary>
public sealed class CourseCompletedRule : ICourseCompletionBadgeRule
{
    public IReadOnlyList<BadgeAwardProposal> Evaluate(CourseEnrollment enrollment) =>
        new[] { new BadgeAwardProposal(BadgeCode.CourseDone, enrollment.CourseId) };
}
