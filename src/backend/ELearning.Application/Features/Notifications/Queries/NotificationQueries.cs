using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.DTOs;

namespace ELearning.Application.Features.Notifications.Queries;

public class GetMyNotificationsQuery : IQuery<List<NotificationDto>> { }
public class GetMyNotificationsHandler : IQueryHandler<GetMyNotificationsQuery, List<NotificationDto>> { 
    Task<Result<List<NotificationDto>>> IQueryHandler<GetMyNotificationsQuery, List<NotificationDto>>.HandleAsync(GetMyNotificationsQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class GetUnreadNotificationsCountQuery : IQuery<int> { }
public class GetUnreadNotificationsCountHandler : IQueryHandler<GetUnreadNotificationsCountQuery, int> { 
    Task<Result<int>> IQueryHandler<GetUnreadNotificationsCountQuery, int>.HandleAsync(GetUnreadNotificationsCountQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
