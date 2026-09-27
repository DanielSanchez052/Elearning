using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Notifications.Services;

public class NotificationPublisher : INotificationPublisher
{
    private readonly INotificationRepository _notifications;

    public NotificationPublisher(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task PublishAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? referenceId = null,
        CancellationToken ct = default)
    {
        var notification = Notification.Create(userId, type, title, message, referenceId);
        await _notifications.AddAsync(notification, ct);
    }
}
