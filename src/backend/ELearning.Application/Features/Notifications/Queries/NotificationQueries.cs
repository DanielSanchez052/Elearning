using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Notifications.Queries;

public class GetMyNotificationsQuery : IQuery<List<NotificationDto>> { public Guid UserId { get; set; } }
public class GetMyNotificationsHandler : IQueryHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    private readonly INotificationRepository _notifications;

    public GetMyNotificationsHandler(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<Result<List<NotificationDto>>> HandleAsync(GetMyNotificationsQuery query, CancellationToken ct = default)
    {
        var notifications = await _notifications.GetByUserAsync(query.UserId, ct);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type.ToString(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return Result.Success(dtos);
    }
}

public class GetUnreadNotificationsCountQuery : IQuery<int> { public Guid UserId { get; set; } }
public class GetUnreadNotificationsCountHandler : IQueryHandler<GetUnreadNotificationsCountQuery, int>
{
    private readonly INotificationRepository _notifications;

    public GetUnreadNotificationsCountHandler(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<Result<int>> HandleAsync(GetUnreadNotificationsCountQuery query, CancellationToken ct = default)
    {
        var count = await _notifications.GetUnreadCountAsync(query.UserId, ct);
        return Result.Success(count);
    }
}
