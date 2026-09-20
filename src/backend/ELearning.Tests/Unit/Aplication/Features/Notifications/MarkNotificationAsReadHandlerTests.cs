using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Notifications;

public class MarkNotificationAsReadHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationsMock = new();
    private readonly MarkNotificationAsReadHandler _handler;

    public MarkNotificationAsReadHandlerTests() =>
        _handler = new MarkNotificationAsReadHandler(_notificationsMock.Object);

    [Fact]
    public async Task HandleAsync_NotificationNotFound_ReturnsNotFound()
    {
        var notificationId = Guid.NewGuid();

        _notificationsMock
            .Setup(r => r.GetByIdAsync(notificationId, default))
            .ReturnsAsync((Notification?)null);

        var result = await _handler.HandleAsync(
            new MarkNotificationAsReadCommand { UserId = Guid.NewGuid(), NotificationId = notificationId });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _notificationsMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NotificationBelongsToAnotherUser_ReturnsForbiddenAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var notification = Notification.Create(ownerId, NotificationType.Reminder, "Recordatorio", "Mensaje");

        _notificationsMock
            .Setup(r => r.GetByIdAsync(notification.Id, default))
            .ReturnsAsync(notification);

        var result = await _handler.HandleAsync(
            new MarkNotificationAsReadCommand { UserId = requesterId, NotificationId = notification.Id });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.False(notification.IsRead);
        _notificationsMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidOwnedNotification_MarksAsReadAndSavesOnce()
    {
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationType.BadgeEarned, "Insignia", "Ganaste una insignia");

        _notificationsMock
            .Setup(r => r.GetByIdAsync(notification.Id, default))
            .ReturnsAsync(notification);
        _notificationsMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(
            new MarkNotificationAsReadCommand { UserId = userId, NotificationId = notification.Id });

        Assert.True(result.IsSuccess);
        Assert.True(notification.IsRead);
        _notificationsMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
