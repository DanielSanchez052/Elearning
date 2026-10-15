using ELearning.Application.Features.Gamification.Rules;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Application.Features.Notifications.Messages;

/// <summary>Sent when a user earns a badge. ReferenceId: the awarded <see cref="UserBadge"/>.</summary>
public sealed record BadgeEarnedMessage(Badge Badge, Guid UserBadgeId)
    : NotificationMessage(NotificationType.BadgeEarned, UserBadgeId)
{
    public override string Title => $"¡Medalla obtenida: {Badge.Name}!";

    public override string Body => Badge.Code switch
    {
        nameof(BadgeCode.LoginFirst) =>
            "Iniciaste sesión en la plataforma por primera vez. ¡Te damos la bienvenida!",
        nameof(BadgeCode.CourseDone) =>
            "Completaste un curso. ¡Sigue así! Puedes ver tus medallas en tu perfil.",
        nameof(BadgeCode.Speedster) =>
            $"¡Eres un Velocista! Aprobaste el examen final en menos de {SpeedsterRule.Threshold.TotalMinutes:0} minutos, en tu primer intento.",
        _ => Badge.Description ?? "Obtuviste una nueva medalla. Puedes verla en tu perfil."
    };
}
