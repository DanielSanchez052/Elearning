using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Notifications.Services;

/// <summary>
/// Reusable seam for staging in-app notifications from server-side code
/// (badge earned, new course published, pending-course reminder, ...).
/// Only stages the row via the repository — it does NOT call
/// SaveChangesAsync. The caller is expected to persist it as part of its
/// own existing transaction/SaveChangesAsync call.
///
/// Email and real-time (SignalR) delivery are explicitly out of scope for
/// this slice; this interface only covers the in-app notification channel.
/// </summary>
public interface INotificationPublisher
{
    Task PublishAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? referenceId = null,
        CancellationToken ct = default);
}
