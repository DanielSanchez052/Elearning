using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Enrollments.Commands;

/// <summary>
/// Cualquier rol puede inscribirse, pero solo en cursos disponibles en su país
/// (publicados y globales o asignados a él; ver <see cref="CourseAccess.IsAvailableIn"/>).
/// Si no lo está, responde 404 como el detalle del curso, sin revelar que existe.
/// </summary>
public record EnrollInCourseCommand(Guid UserId, Guid CourseId, int UserCountryId) : ICommand<Guid>;

public class EnrollInCourseHandler : ICommandHandler<EnrollInCourseCommand, Guid>
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICourseRepository _courses;

    public EnrollInCourseHandler(IEnrollmentRepository enrollments, ICourseRepository courses)
    {
        _enrollments = enrollments;
        _courses = courses;
    }

    public async Task<Result<Guid>> HandleAsync(EnrollInCourseCommand command, CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(command.CourseId, ct);

        if (course is null || !CourseAccess.IsAvailableIn(course, command.UserCountryId))
            return Result.NotFound<Guid>($"Curso con id '{command.CourseId}' no encontrado.");

        var alreadyEnrolled = await _enrollments.ExistsAsync(command.UserId, command.CourseId, ct);
        if (alreadyEnrolled)
            return Result.Conflict<Guid>("El usuario ya está inscrito en este curso.");

        var enrollment = CourseEnrollment.Create(command.UserId, command.CourseId);

        await _enrollments.AddAsync(enrollment, ct);
        await _enrollments.SaveChangesAsync(ct);

        return enrollment.Id;
    }
}
