using ELearning.Application.Features.Notifications.Messages;
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
    public async Task PublishAsync_StagesNotificationBuiltFromTheMessage()
    {
        var recipientId = Guid.NewGuid();
        var course = Course.Create("Excel Avanzado", null, null, Guid.NewGuid(), isGlobal: false);
        var message = new CourseAssignedMessage(course);
        Notification? captured = null;
        _notificationsMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), default))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        await _publisher.PublishAsync(recipientId, message);

        _notificationsMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), default), Times.Once);
        Assert.NotNull(captured);
        Assert.Equal(recipientId, captured!.UserId);
        Assert.Equal(NotificationType.CourseAssigned, captured.Type);
        Assert.Equal(message.Title, captured.Title);
        Assert.Equal(message.Body, captured.Message);
        Assert.Equal(course.Id, captured.ReferenceId);
        Assert.False(captured.IsRead);
    }

    [Fact]
    public async Task PublishAsync_DoesNotCallSaveChangesAsync()
    {
        _notificationsMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), default))
            .ReturnsAsync((Notification n, CancellationToken _) => n);
        var course = Course.Create("Curso", null, null, Guid.NewGuid(), isGlobal: false);

        await _publisher.PublishAsync(Guid.NewGuid(), new CourseUnassignedMessage(course));

        _notificationsMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
