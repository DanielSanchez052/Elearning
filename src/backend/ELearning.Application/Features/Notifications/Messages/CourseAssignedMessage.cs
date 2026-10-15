using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Notifications.Messages;

/// <summary>Sent to the instructor who was just assigned to a course. ReferenceId: the course.</summary>
public sealed record CourseAssignedMessage(Course Course)
    : NotificationMessage(NotificationType.CourseAssigned, Course.Id)
{
    public override string Title => "Te asignaron un curso";

    public override string Body => $"Ahora eres el instructor del curso «{Course.Title}».";
}
