using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// Proposes Speedster ("Velocista"), scoped to the exam's course, when a
/// course final exam was passed on the FIRST attempt in strictly less than
/// 10 minutes (measured from the ExamSession start to the submission).
/// Results without a known start time (Duration == null) never qualify.
/// Once per course is enforced by the service's HasBadgeAsync check.
/// </summary>
public sealed class SpeedsterRule : IExamPassedBadgeRule
{
    public static readonly TimeSpan Threshold = TimeSpan.FromMinutes(10);

    public IReadOnlyList<BadgeAwardProposal> Evaluate(UserQuizResult result)
    {
        var qualifies =
            result.CourseId is not null &&
            result.IsPassed &&
            result.AttemptNumber == 1 &&
            result.Duration is { } duration &&
            duration < Threshold;

        return qualifies
            ? new[] { new BadgeAwardProposal(BadgeCode.Speedster, result.CourseId) }
            : Array.Empty<BadgeAwardProposal>();
    }
}
