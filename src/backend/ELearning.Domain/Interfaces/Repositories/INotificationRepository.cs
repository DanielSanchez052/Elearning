using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Todas las notificaciones del usuario, ordenadas por CreatedAt descendente.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
