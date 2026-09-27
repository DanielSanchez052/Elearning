using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Gamification.Rules;

/// <summary>
/// A rule's proposal that a badge be awarded. The service (not the rule)
/// decides whether the badge already exists / was already earned — rules
/// stay pure and don't need to check idempotency themselves.
/// </summary>
/// <param name="Code">Which badge to award.</param>
/// <param name="CourseId">
/// Scope for course-bound badges (CourseDone, Speedster). Null for
/// course-agnostic badges (LoginFirst).
/// </param>
public sealed record BadgeAwardProposal(BadgeCode Code, Guid? CourseId = null);
