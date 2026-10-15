using ELearning.Application.Features.Notifications.Messages;
using ELearning.Application.Features.Notifications.Services;
using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Courses.Services;

public sealed class CourseInstructorNotifier : ICourseInstructorNotifier
{
    private readonly INotificationPublisher _notifications;

    public CourseInstructorNotifier(INotificationPublisher notifications)
    {
        _notifications = notifications;
    }

    public async Task InstructorChangedAsync(
        Course course, Guid? previousInstructorId, Guid? newInstructorId, CancellationToken ct = default)
    {
        if (previousInstructorId == newInstructorId)
            return;

        if (previousInstructorId is { } previous)
            await _notifications.PublishAsync(previous, new CourseUnassignedMessage(course), ct);

        if (newInstructorId is { } next)
            await _notifications.PublishAsync(next, new CourseAssignedMessage(course), ct);
    }
}
