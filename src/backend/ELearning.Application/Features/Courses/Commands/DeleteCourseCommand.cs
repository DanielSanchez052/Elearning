using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Commands;

public sealed record DeleteCourseCommand(
    Guid CourseId,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : ICommand;

public sealed class DeleteCourseHandler : ICommandHandler<DeleteCourseCommand>
{
    private readonly ICourseRepository _courses;

    public DeleteCourseHandler(ICourseRepository courses)
    {
        _courses = courses;
    }

    public async Task<Result> HandleAsync(
        DeleteCourseCommand cmd,
        CancellationToken ct = default)
    {
        var course = await _courses.GetByIdTrackedAsync(cmd.CourseId, ct);
        if (course is null)
            return Result.NotFound($"Curso con id '{cmd.CourseId}' no encontrado.");

        if (!CourseAccess.CanManage(course, cmd.RequesterId, cmd.RequesterRole, cmd.RequesterCountryId))
            return Result.Forbidden(CourseAccess.ForbiddenMessage);

        // No se puede eliminar un curso publicado con estudiantes inscritos
        if (course.IsActive && course.Enrollments.Any())
            return Result.Conflict(
                "No puedes eliminar un curso publicado con estudiantes inscritos. " +
                "Despublícalo primero.");

        // Soft delete — desactivar en lugar de borrar físicamente
        course.Deactivate();
        await _courses.UpdateAsync(course, ct);

        return Result.Success();
    }
}
