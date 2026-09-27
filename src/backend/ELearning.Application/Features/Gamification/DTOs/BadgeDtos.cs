namespace ELearning.Application.Features.Gamification.DTOs;

/// <summary>
/// A badge that was newly awarded as the direct result of a triggering action
/// (login, course completion, exam passed). Returned by IBadgeAwardService so
/// callers *could* build an instant-toast response later; unused for now per
/// the locked design (no AwardedBadges field on any response contract yet —
/// awarding relies on the existing notification polling instead).
/// </summary>
public sealed record AwardedBadgeDto(
    int BadgeId,
    string Code,
    string Name,
    Guid? CourseId,
    DateTime ObtainedAt
);

/// <summary>One of the calling user's earned badges, for GET /api/badges/me.</summary>
public sealed record UserBadgeDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    DateTime ObtainedAt,
    Guid? CourseId,
    string? CourseTitle
);
