using ELearning.Application.Features.Notifications.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Notifications;

public class NotificationPublisherTests
{
    private readonly Mock<INotificationRepository> _notificationsMock = new();
    private readonly NotificationPublisher _publisher;

    public NotificationPublisherTests() =>
        _publisher = new NotificationPublisher(_notificationsMock.Object);

    [Fact]
    public async Task PublishAsync_CallsAddAsync_WithCorrectlyPopulatedNotification()
    {
        var userId = Guid.NewGuid();
        var referenceId = Guid.NewGuid();
        Notification? captured = null;

        _notificationsMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), default))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        await _publisher.PublishAsync(
            userId,
            NotificationType.BadgeEarned,
            "¡Insignia!",
            "Ganaste una insignia",
            referenceId);

        _notificationsMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), default), Times.Once);

        Assert.NotNull(captured);
        Assert.Equal(userId, captured!.UserId);
        Assert.Equal(NotificationType.BadgeEarned, captured.Type);
        Assert.Equal("¡Insignia!", captured.Title);
        Assert.Equal("Ganaste una insignia", captured.Message);
        Assert.Equal(referenceId, captured.ReferenceId);
    }

    [Fact]
    public async Task PublishAsync_NullReferenceId_CreatesNotificationWithNullReferenceId()
    {
        var userId = Guid.NewGuid();
        Notification? captured = null;

        _notificationsMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), default))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        await _publisher.PublishAsync(userId, NotificationType.NewCourse, "Curso nuevo", "Hay un curso nuevo", null);

        Assert.NotNull(captured);
        Assert.Null(captured!.ReferenceId);
    }

    [Fact]
    public async Task PublishAsync_DoesNotCallSaveChangesAsync()
    {
        var userId = Guid.NewGuid();

        _notificationsMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), default))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        await _publisher.PublishAsync(userId, NotificationType.BadgeEarned, "Title", "Message", null);

        _notificationsMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
