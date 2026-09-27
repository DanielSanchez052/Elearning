using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// Evaluated by BadgeAwardService.OnCourseExamPassedAsync. Pure function: takes
/// the just-created course-exam result (which carries CourseId, AttemptNumber,
/// IsPassed and Duration), proposes zero or more badges. Does not check whether
/// the badge was already awarded — that's the service's job.
/// </summary>
public interface IExamPassedBadgeRule
{
    IReadOnlyList<BadgeAwardProposal> Evaluate(UserQuizResult result);
}
