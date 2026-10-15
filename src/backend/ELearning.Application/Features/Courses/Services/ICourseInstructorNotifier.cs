using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Courses.Services;

/// <summary>
/// Decide quién recibe aviso cuando cambia el instructor de un curso: el que sale y el que entra.
/// Qué dice cada aviso lo definen los mensajes de Notifications (CourseAssignedMessage,
/// CourseUnassignedMessage).
/// </summary>
public interface ICourseInstructorNotifier
{
    /// <summary>
    /// Prepara los avisos sin guardarlos: son parte de la asignación, así que quien llama los
    /// persiste con el SaveChanges del curso (ver la regla en INotificationPublisher).
    /// No hace nada si el instructor no cambia.
    /// </summary>
    Task InstructorChangedAsync(
        Course course, Guid? previousInstructorId, Guid? newInstructorId, CancellationToken ct = default);
}
