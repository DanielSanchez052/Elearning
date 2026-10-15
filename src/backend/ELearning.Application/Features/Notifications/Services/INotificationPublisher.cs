using ELearning.Application.Features.Notifications.Messages;

namespace ELearning.Application.Features.Notifications.Services;

/// <summary>
/// Reusable seam for staging in-app notifications from server-side code. Each kind of
/// notification is a <see cref="NotificationMessage"/> (see the Messages folder), so callers
/// say what happened and the copy lives with the message, not with the caller.
///
/// It only stages the row via the repository — it does NOT call SaveChangesAsync. The caller
/// decides how it is persisted, based on how much the notification matters to its action:
/// <list type="bullet">
///   <item><b>Part of the action</b> (e.g. the instructor assigned to a course): publish before
///   the action's own SaveChangesAsync, so both are saved in the same transaction.</item>
///   <item><b>Secondary</b> (e.g. badges): publish after the action is saved, guarded by a
///   try/catch, and save separately, so a failed notification never fails the action.</item>
/// </list>
///
/// Email and real-time (SignalR) delivery are out of scope; this only covers the in-app channel.
/// </summary>
public interface INotificationPublisher
{
    Task PublishAsync(Guid recipientId, NotificationMessage message, CancellationToken ct = default);
}
