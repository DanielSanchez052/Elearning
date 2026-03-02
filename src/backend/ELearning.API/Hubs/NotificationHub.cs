using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace ELearning.API.Hubs;

public interface INotificationClient
{
    Task ReceiveNotification(string title, string message, string type);
    Task BadgeEarned(string badgeName, string badgeIcon);
    Task CourseCompleted(string courseName);
    Task NewCourseAvailable(string courseName, string countryName);
}

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    private static readonly Dictionary<string, string> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst("userId")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = Context.ConnectionId;
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst("userId")?.Value;
        
        if (!string.IsNullOrEmpty(userId) && _userConnections.ContainsKey(userId))
        {
            _userConnections.Remove(userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionId(string userId)
    {
        return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
    }

    public async Task SendNotificationToUser(string userId, string title, string message, string type)
    {
        var connectionId = GetConnectionId(userId);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await Clients.Client(connectionId).ReceiveNotification(title, message, type);
        }
    }

    public async Task NotifyBadgeEarned(string userId, string badgeName, string badgeIcon)
    {
        var connectionId = GetConnectionId(userId);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await Clients.Client(connectionId).BadgeEarned(badgeName, badgeIcon);
        }
    }

    public async Task NotifyCourseCompleted(string userId, string courseName)
    {
        var connectionId = GetConnectionId(userId);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await Clients.Client(connectionId).CourseCompleted(courseName);
        }
    }

    public async Task NotifyNewCourse(string userId, string courseName, string countryName)
    {
        var connectionId = GetConnectionId(userId);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await Clients.Client(connectionId).NewCourseAvailable(courseName, countryName);
        }
    }
}
