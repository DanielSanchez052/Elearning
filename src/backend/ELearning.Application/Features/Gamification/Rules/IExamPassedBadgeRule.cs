using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// Contract for the future Speedster rule (Track A / Integration): awards
/// Speedster when a course-final-exam result was passed within the time
/// threshold on the first attempt. No implementation exists yet — it needs
/// UserQuizResult.Duration, which lands with Track A's ExamSession work.
/// Not registered in DI and not referenced by BadgeAwardService yet;
/// OnCourseExamPassedAsync is a documented no-op stub until Integration
/// wires SpeedsterRule through this contract.
/// </summary>
public interface IExamPassedBadgeRule
{
    IReadOnlyList<BadgeAwardProposal> Evaluate(UserQuizResult result);
}
