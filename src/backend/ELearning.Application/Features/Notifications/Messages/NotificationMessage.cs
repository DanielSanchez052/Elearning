using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Notifications.Messages;

/// <summary>
/// An in-app notification as the user receives it: its type, the copy they read and what
/// <see cref="ReferenceId"/> points to. Each kind of notification is its own record in this
/// folder, so every user-facing text lives in one place and a type can never be published
/// with another notification's copy.
/// </summary>
public abstract record NotificationMessage(NotificationType Type, Guid? ReferenceId)
{
    /// <summary>Stored in <c>notifications.title</c> (varchar 150).</summary>
    public abstract string Title { get; }

    public abstract string Body { get; }
}
