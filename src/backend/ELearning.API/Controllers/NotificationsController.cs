using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Notifications.Commands;
using ELearning.Application.Features.Notifications.DTOs;
using ELearning.Application.Features.Notifications.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(
    IQueryHandler<GetMyNotificationsQuery, List<NotificationDto>> getMyNotificationsHandler,
    IQueryHandler<GetUnreadNotificationsCountQuery, int> getUnreadCountHandler,
    ICommandHandler<MarkNotificationAsReadCommand> markNotificationAsReadHandler,
    ICommandHandler<MarkAllNotificationsAsReadCommand> markAllNotificationsAsReadHandler
) : ControllerBase
{
    // ── GET /api/notifications ──────────────────────────────────────────────────
    // Obtener las notificaciones del usuario autenticado

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await getMyNotificationsHandler.HandleAsync(new GetMyNotificationsQuery { UserId = userId }, ct);
        return this.ToActionResult(result);
    }

    // ── GET /api/notifications/unread-count ────────────────────────────────────
    // Obtener la cantidad de notificaciones no leídas

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await getUnreadCountHandler.HandleAsync(new GetUnreadNotificationsCountQuery { UserId = userId }, ct);
        return this.ToActionResult(result);
    }

    // ── PUT /api/notifications/{id}/mark-read ──────────────────────────────────
    // Marcar una notificación específica como leída

    [HttpPut("{id:guid}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await markNotificationAsReadHandler.HandleAsync(
            new MarkNotificationAsReadCommand { UserId = userId, NotificationId = id }, ct);
        return this.ToActionResult(result);
    }

    // ── PUT /api/notifications/mark-all-read ───────────────────────────────────
    // Marcar todas las notificaciones del usuario como leídas

    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await markAllNotificationsAsReadHandler.HandleAsync(
            new MarkAllNotificationsAsReadCommand { UserId = userId }, ct);
        return this.ToActionResult(result);
    }
}
