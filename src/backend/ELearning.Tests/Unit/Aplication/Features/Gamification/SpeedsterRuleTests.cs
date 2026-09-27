using ELearning.Application.Features.Gamification.Rules;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

public class SpeedsterRuleTests
{
    private readonly SpeedsterRule _rule = new();

    private static readonly DateTime StartedAt = new(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Builds a course-exam result with an exact Duration: CompletedAt is set by
    /// Create() to "now", so it is overwritten via reflection to StartedAt + duration.
    /// </summary>
    private static UserQuizResult BuildResult(
        TimeSpan? duration,
        bool passed = true,
        int attemptNumber = 1,
        Guid? courseId = null,
        bool courseScoped = true)
    {
        var result = UserQuizResult.Create(
            userId: Guid.NewGuid(),
            lessonId: courseScoped ? null : Guid.NewGuid(),
            courseId: courseScoped ? courseId ?? Guid.NewGuid() : null,
            attemptNumber: attemptNumber,
            score: passed ? 100m : 0m,
            passScore: 70m,
            startedAt: duration is null ? null : StartedAt);

        if (duration is not null)
            GamificationTestHelpers.SetPrivate(result, nameof(UserQuizResult.CompletedAt), StartedAt + duration.Value);

        return result;
    }

    [Fact]
    public void Evaluate_PassedFirstAttemptUnderTenMinutes_ProposesSpeedsterScopedToCourse()
    {
        var courseId = Guid.NewGuid();
        var result = BuildResult(new TimeSpan(0, 9, 59), courseId: courseId);

        var proposals = _rule.Evaluate(result);

        var proposal = Assert.Single(proposals);
        Assert.Equal(BadgeCode.Speedster, proposal.Code);
        Assert.Equal(courseId, proposal.CourseId);
    }

    [Fact]
    public void Evaluate_ExactlyTenMinutes_ProposesNothing()
    {
        var result = BuildResult(TimeSpan.FromMinutes(10));

        Assert.Empty(_rule.Evaluate(result));
    }

    [Fact]
    public void Evaluate_UnderTenMinutesButSecondAttempt_ProposesNothing()
    {
        var result = BuildResult(new TimeSpan(0, 9, 59), attemptNumber: 2);

        Assert.Empty(_rule.Evaluate(result));
    }

    [Fact]
    public void Evaluate_UnderTenMinutesButNotPassed_ProposesNothing()
    {
        var result = BuildResult(new TimeSpan(0, 9, 59), passed: false);

        Assert.Empty(_rule.Evaluate(result));
    }

    [Fact]
    public void Evaluate_LessonQuizResultWithoutCourseId_ProposesNothing()
    {
        var result = BuildResult(new TimeSpan(0, 9, 59), courseScoped: false);

        Assert.Empty(_rule.Evaluate(result));
    }

    [Fact]
    public void Evaluate_UnknownDuration_ProposesNothing()
    {
        var result = BuildResult(duration: null);

        Assert.Null(result.Duration);
        Assert.Empty(_rule.Evaluate(result));
    }
}
