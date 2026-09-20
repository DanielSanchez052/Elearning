using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.Queries;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Notifications;

public class GetUnreadNotificationsCountHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationsMock = new();
    private readonly GetUnreadNotificationsCountHandler _handler;

    public GetUnreadNotificationsCountHandlerTests() =>
        _handler = new GetUnreadNotificationsCountHandler(_notificationsMock.Object);

    [Fact]
    public async Task HandleAsync_NoUnreadNotifications_ReturnsZero()
    {
        var userId = Guid.NewGuid();

        _notificationsMock
            .Setup(r => r.GetUnreadCountAsync(userId, default))
            .ReturnsAsync(0);

        var result = await _handler.HandleAsync(new GetUnreadNotificationsCountQuery { UserId = userId });

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task HandleAsync_MixedReadAndUnread_ReturnsCorrectCount()
    {
        var userId = Guid.NewGuid();

        _notificationsMock
            .Setup(r => r.GetUnreadCountAsync(userId, default))
            .ReturnsAsync(3);

        var result = await _handler.HandleAsync(new GetUnreadNotificationsCountQuery { UserId = userId });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value);
    }
}
