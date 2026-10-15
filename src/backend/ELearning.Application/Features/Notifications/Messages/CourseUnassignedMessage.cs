using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Notifications.Messages;

/// <summary>Sent to the instructor who stopped being a course's instructor. ReferenceId: the course.</summary>
public sealed record CourseUnassignedMessage(Course Course)
    : NotificationMessage(NotificationType.CourseUnassigned, Course.Id)
{
    public override string Title => "Ya no tienes este curso asignado";

    public override string Body => $"Dejaste de ser el instructor del curso «{Course.Title}».";
}
