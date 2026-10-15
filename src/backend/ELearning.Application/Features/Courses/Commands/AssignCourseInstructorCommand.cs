using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Application.Features.Courses.Services;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Commands;

/// <summary>
/// Asigna (o quita, con <c>InstructorId = null</c>) el instructor responsable de un curso.
/// Lo hacen admin y superadmin sobre los cursos que gestionan; el admin solo puede elegir
/// instructores de su país. Avisa en la app al instructor que entra y al que sale.
/// </summary>
public sealed record AssignCourseInstructorCommand(
    Guid CourseId,
    Guid? InstructorId,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : ICommand;

public sealed class AssignCourseInstructorHandler : ICommandHandler<AssignCourseInstructorCommand>
{
    private readonly ICourseRepository _courses;
    private readonly IUserRepository _users;
    private readonly ICourseInstructorNotifier _instructorNotifier;

    public AssignCourseInstructorHandler(
        ICourseRepository courses, IUserRepository users, ICourseInstructorNotifier instructorNotifier)
    {
        _courses = courses;
        _users = users;
        _instructorNotifier = instructorNotifier;
    }

    public async Task<Result> HandleAsync(AssignCourseInstructorCommand cmd, CancellationToken ct = default)
    {
        var course = await _courses.GetByIdTrackedAsync(cmd.CourseId, ct);
        if (course is null)
            return Result.NotFound($"Curso con id '{cmd.CourseId}' no encontrado.");

        if (!CourseAccess.CanAssignInstructor(course, cmd.RequesterId, cmd.RequesterRole, cmd.RequesterCountryId))
            return Result.Forbidden(CourseAccess.AssignInstructorForbiddenMessage);

        if (cmd.InstructorId is { } instructorId)
        {
            // Un usuario que el admin no puede ver (de otro país) responde 404, como en el resto
            // de la gestión de usuarios; uno visible que no es instructor es un error de datos.
            var candidate = await _users.GetByIdAsync(instructorId, ct);
            if (candidate is null || !UserAccess.CanView(candidate, cmd.RequesterRole, cmd.RequesterCountryId))
                return Result.NotFound($"Usuario con id '{instructorId}' no encontrado.");
            if (!CourseAccess.IsEligibleInstructor(candidate, cmd.RequesterRole, cmd.RequesterCountryId))
                return Result.ValidationFailure("El usuario elegido no tiene rol de instructor.");
        }

        var previousInstructorId = course.InstructorId;
        if (previousInstructorId == cmd.InstructorId)
            return Result.Success();

        course.AssignInstructor(cmd.InstructorId);
        await _instructorNotifier.InstructorChangedAsync(course, previousInstructorId, cmd.InstructorId, ct);
        await _courses.UpdateAsync(course, ct);

        return Result.Success();
    }
}
