using ELearning.Application.Features.Notifications.Messages;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Notifications.Services;

public class NotificationPublisher : INotificationPublisher
{
    private readonly INotificationRepository _notifications;

    public NotificationPublisher(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task PublishAsync(Guid recipientId, NotificationMessage message, CancellationToken ct = default)
    {
        var notification = Notification.Create(
            recipientId, message.Type, message.Title, message.Body, message.ReferenceId);
        await _notifications.AddAsync(notification, ct);
    }
}
