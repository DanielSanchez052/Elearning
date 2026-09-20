using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Notifications;

public class GetMyNotificationsHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationsMock = new();
    private readonly GetMyNotificationsHandler _handler;

    public GetMyNotificationsHandlerTests() =>
        _handler = new GetMyNotificationsHandler(_notificationsMock.Object);

    [Fact]
    public async Task HandleAsync_NoNotifications_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();

        _notificationsMock
            .Setup(r => r.GetByUserAsync(userId, default))
            .ReturnsAsync(new List<Notification>());

        var result = await _handler.HandleAsync(new GetMyNotificationsQuery { UserId = userId });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_HasNotifications_ReturnsCorrectlyMappedDtosOrderedByCreatedAtDesc()
    {
        var userId = Guid.NewGuid();

        var older = Notification.Create(userId, NotificationType.NewCourse, "Curso nuevo", "Hay un curso nuevo disponible");
        var newer = Notification.Create(userId, NotificationType.BadgeEarned, "¡Insignia!", "Ganaste una insignia");
        newer.MarkAsRead();

        // El repositorio es responsable del orden (CreatedAt desc); el handler debe
        // preservar el orden en que el repositorio los devuelve.
        var orderedByRepo = new List<Notification> { newer, older };

        _notificationsMock
            .Setup(r => r.GetByUserAsync(userId, default))
            .ReturnsAsync(orderedByRepo);

        var result = await _handler.HandleAsync(new GetMyNotificationsQuery { UserId = userId });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        var firstDto = result.Value[0];
        Assert.Equal(newer.Id, firstDto.Id);
        Assert.Equal(newer.Title, firstDto.Title);
        Assert.Equal(newer.Message, firstDto.Message);
        Assert.Equal(newer.Type.ToString(), firstDto.Type);
        Assert.True(firstDto.IsRead);
        Assert.Equal(newer.CreatedAt, firstDto.CreatedAt);

        var secondDto = result.Value[1];
        Assert.Equal(older.Id, secondDto.Id);
        Assert.False(secondDto.IsRead);
    }
}
