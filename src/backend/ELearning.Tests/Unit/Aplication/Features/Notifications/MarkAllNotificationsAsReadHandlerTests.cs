using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Notifications;

public class MarkAllNotificationsAsReadHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationsMock = new();
    private readonly MarkAllNotificationsAsReadHandler _handler;

    public MarkAllNotificationsAsReadHandlerTests() =>
        _handler = new MarkAllNotificationsAsReadHandler(_notificationsMock.Object);

    [Fact]
    public async Task HandleAsync_OnlyMarksCallingUsersUnreadNotifications()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myUnread1 = Notification.Create(userId, NotificationType.NewCourse, "Curso 1", "Mensaje 1");
        var myUnread2 = Notification.Create(userId, NotificationType.Reminder, "Recordatorio", "Mensaje 2");
        var othersUnread = Notification.Create(otherUserId, NotificationType.BadgeEarned, "Insignia", "Mensaje otro usuario");

        // El repositorio ya filtra por usuario — el handler solo debe pedir
        // las no leídas de command.UserId, nunca de toda la tabla.
        _notificationsMock
            .Setup(r => r.GetUnreadByUserAsync(userId, default))
            .ReturnsAsync(new List<Notification> { myUnread1, myUnread2 });

        var result = await _handler.HandleAsync(new MarkAllNotificationsAsReadCommand { UserId = userId });

        Assert.True(result.IsSuccess);
        Assert.True(myUnread1.IsRead);
        Assert.True(myUnread2.IsRead);
        Assert.False(othersUnread.IsRead);
        _notificationsMock.Verify(r => r.GetUnreadByUserAsync(userId, default), Times.Once);
        _notificationsMock.Verify(r => r.GetUnreadByUserAsync(otherUserId, default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoUnreadNotifications_StillSavesAndSucceeds()
    {
        var userId = Guid.NewGuid();

        _notificationsMock
            .Setup(r => r.GetUnreadByUserAsync(userId, default))
            .ReturnsAsync(new List<Notification>());

        var result = await _handler.HandleAsync(new MarkAllNotificationsAsReadCommand { UserId = userId });

        Assert.True(result.IsSuccess);
        _notificationsMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AlreadyReadNotificationsAreNeverFetchedForMarking()
    {
        var userId = Guid.NewGuid();
        var unread = Notification.Create(userId, NotificationType.NewCourse, "Curso", "Mensaje");

        // El repositorio (GetUnreadByUserAsync) ya excluye las leídas; el handler
        // confía en ese filtro y solo procesa lo que recibe.
        _notificationsMock
            .Setup(r => r.GetUnreadByUserAsync(userId, default))
            .ReturnsAsync(new List<Notification> { unread });
        _notificationsMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new MarkAllNotificationsAsReadCommand { UserId = userId });

        Assert.True(result.IsSuccess);
        Assert.True(unread.IsRead);
        _notificationsMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
