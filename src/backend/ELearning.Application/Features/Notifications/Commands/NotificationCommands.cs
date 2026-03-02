using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.DTOs;

namespace ELearning.Application.Features.Notifications.Commands;

public class MarkNotificationAsReadCommand : ICommand { public Guid NotificationId { get; set; } }
public class MarkNotificationAsReadHandler : ICommandHandler<MarkNotificationAsReadCommand> { 
    Task<Result> ICommandHandler<MarkNotificationAsReadCommand>.HandleAsync(MarkNotificationAsReadCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class MarkAllNotificationsAsReadCommand : ICommand { }
public class MarkAllNotificationsAsReadHandler : ICommandHandler<MarkAllNotificationsAsReadCommand> {
    Task<Result> ICommandHandler<MarkAllNotificationsAsReadCommand>.HandleAsync(MarkAllNotificationsAsReadCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
