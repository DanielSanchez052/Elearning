using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Repositories;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Lesson?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Todas las lecciones de un curso ordenadas por OrderIndex.
    /// </summary>
    Task<IReadOnlyList<Lesson>> GetByCourseAsync(
        Guid courseId,
        CancellationToken ct = default);

    /// <summary>
    /// OrderIndex más alto actual del curso — para calcular el siguiente al crear.
    /// </summary>
    Task<int> GetMaxOrderIndexAsync(
        Guid courseId,
        CancellationToken ct = default);

    Task CreateAsync(Lesson lesson, CancellationToken ct = default);
    Task UpdateAsync(Lesson lesson, CancellationToken ct = default);
    Task DeleteAsync(Lesson lesson, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el OrderIndex de múltiples lecciones en una sola operación.
    /// Usado por el comando de reordenamiento.
    /// </summary>
    Task UpdateOrdersAsync(
        IEnumerable<(Guid LessonId, int NewOrder)> orders,
        CancellationToken ct = default);
}