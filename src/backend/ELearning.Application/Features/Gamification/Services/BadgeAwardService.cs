using ELearning.Application.Features.Gamification.DTOs;
using ELearning.Application.Features.Gamification.Rules;
using ELearning.Application.Features.Notifications.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ELearning.Application.Features.Gamification.Services;

public sealed class BadgeAwardService : IBadgeAwardService
{
    private readonly IBadgeRepository _badges;
    private readonly IReadOnlyList<ILoginBadgeRule> _loginRules;
    private readonly IReadOnlyList<ICourseCompletionBadgeRule> _courseCompletionRules;
    private readonly IReadOnlyList<IExamPassedBadgeRule> _examPassedRules;
    private readonly INotificationPublisher _notifications;
    private readonly ILogger<BadgeAwardService> _logger;

    public BadgeAwardService(
        IBadgeRepository badges,
        IEnumerable<ILoginBadgeRule> loginRules,
        IEnumerable<ICourseCompletionBadgeRule> courseCompletionRules,
        IEnumerable<IExamPassedBadgeRule> examPassedRules,
        INotificationPublisher notifications,
        ILogger<BadgeAwardService> logger)
    {
        _badges = badges;
        _loginRules = loginRules.ToList();
        _courseCompletionRules = courseCompletionRules.ToList();
        _examPassedRules = examPassedRules.ToList();
        _notifications = notifications;
        _logger = logger;
    }

    public Task<IReadOnlyList<AwardedBadgeDto>> OnUserLoggedInAsync(User user, CancellationToken ct = default)
    {
        var proposals = _loginRules.SelectMany(rule => rule.Evaluate(user));
        return AwardBestEffortAsync(user.Id, proposals, nameof(OnUserLoggedInAsync), ct);
    }

    public Task<IReadOnlyList<AwardedBadgeDto>> OnCourseCompletedAsync(CourseEnrollment enrollment, CancellationToken ct = default)
    {
        var proposals = _courseCompletionRules.SelectMany(rule => rule.Evaluate(enrollment));
        return AwardBestEffortAsync(enrollment.UserId, proposals, nameof(OnCourseCompletedAsync), ct);
    }

    public Task<IReadOnlyList<AwardedBadgeDto>> OnCourseExamPassedAsync(UserQuizResult result, CancellationToken ct = default)
    {
        var proposals = _examPassedRules.SelectMany(rule => rule.Evaluate(result));
        return AwardBestEffortAsync(result.UserId, proposals, nameof(OnCourseExamPassedAsync), ct);
    }

    /// <summary>
    /// Awarding never fails the triggering action: any failure is logged here
    /// (the calling handlers have no logger of their own) and reported as
    /// "nothing awarded". Cancellation is not an error and still propagates.
    /// </summary>
    private async Task<IReadOnlyList<AwardedBadgeDto>> AwardBestEffortAsync(
        Guid userId, IEnumerable<BadgeAwardProposal> proposals, string trigger, CancellationToken ct)
    {
        try
        {
            return await AwardAsync(userId, proposals, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Badge award failed for user {UserId} during {Trigger}; the triggering action is unaffected.",
                userId, trigger);
            return Array.Empty<AwardedBadgeDto>();
        }
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

            // The insert already succeeded (and was saved by TryAddAsync's own
            // internal save), so the badge counts as awarded regardless of what
            // happens next — a failure publishing its notification must not
            // lose it, and must not stop the remaining proposals in this batch
            // from being evaluated.
            awarded.Add(new AwardedBadgeDto(badge.Id, badge.Code, badge.Name, proposal.CourseId, userBadge.ObtainedAt));

            try
            {
                // Only a badge that was really inserted gets a notification, so
                // losing the insert race never produces a duplicate "badge
                // earned" message.
                var (title, message) = BuildNotificationCopy(badge);
                await _notifications.PublishAsync(
                    userId, NotificationType.BadgeEarned, title, message, referenceId: userBadge.Id, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Failed to publish badge-earned notification for user {UserId}, badge {BadgeCode}; " +
                    "the badge itself was still awarded.",
                    userId, badge.Code);
            }
        }

        // TryAddAsync already committed each UserBadge (it must save to catch the
        // unique-index race), so the staged notifications need this separate save.
        if (awarded.Count > 0)
            await _badges.SaveChangesAsync(ct);

        return awarded;
    }

    /// <summary>In-app notification copy (Spanish) for a newly earned badge.</summary>
    public static (string Title, string Message) BuildNotificationCopy(Badge badge)
    {
        var title = $"¡Medalla obtenida: {badge.Name}!";

        var message = badge.Code switch
        {
            nameof(BadgeCode.LoginFirst) =>
                "Iniciaste sesión en la plataforma por primera vez. ¡Te damos la bienvenida!",
            nameof(BadgeCode.CourseDone) =>
                "Completaste un curso. ¡Sigue así! Puedes ver tus medallas en tu perfil.",
            nameof(BadgeCode.Speedster) =>
                $"¡Eres un Velocista! Aprobaste el examen final en menos de {SpeedsterRule.Threshold.TotalMinutes:0} minutos, en tu primer intento.",
            _ => badge.Description ?? "Obtuviste una nueva medalla. Puedes verla en tu perfil."
        };

        return (title, message);
    }
}
