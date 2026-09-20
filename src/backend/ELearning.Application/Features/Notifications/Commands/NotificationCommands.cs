using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Notifications.Commands;

public class MarkNotificationAsReadCommand : ICommand { public Guid UserId { get; set; } public Guid NotificationId { get; set; } }
public class MarkNotificationAsReadHandler : ICommandHandler<MarkNotificationAsReadCommand>
{
    private readonly INotificationRepository _notifications;

    public MarkNotificationAsReadHandler(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<Result> HandleAsync(MarkNotificationAsReadCommand command, CancellationToken ct = default)
    {
        var notification = await _notifications.GetByIdAsync(command.NotificationId, ct);
        if (notification is null)
            return Result.NotFound("Notificación no encontrada");

        if (notification.UserId != command.UserId)
            return Result.Forbidden("No tienes permiso para modificar esta notificación");

        notification.MarkAsRead();
        await _notifications.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class MarkAllNotificationsAsReadCommand : ICommand { public Guid UserId { get; set; } }
public class MarkAllNotificationsAsReadHandler : ICommandHandler<MarkAllNotificationsAsReadCommand>
{
    private readonly INotificationRepository _notifications;

    public MarkAllNotificationsAsReadHandler(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<Result> HandleAsync(MarkAllNotificationsAsReadCommand command, CancellationToken ct = default)
    {
        var unread = await _notifications.GetUnreadByUserAsync(command.UserId, ct);

        foreach (var notification in unread)
            notification.MarkAsRead();

        await _notifications.SaveChangesAsync(ct);

        return Result.Success();
    }
}
