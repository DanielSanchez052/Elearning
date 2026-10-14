using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Commands;

public sealed record ToggleCourseStatusCommand(
    Guid CourseId,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : ICommand;

public sealed class ToggleCourseStatusHandler : ICommandHandler<ToggleCourseStatusCommand>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public ToggleCourseStatusHandler(
        ICourseRepository courses,
        ILessonRepository lessons)
    {
        _courses = courses;
        _lessons = lessons;
    }

    public async Task<Result> HandleAsync(
        ToggleCourseStatusCommand cmd,
        CancellationToken ct = default)
    {
        var course = await _courses.GetByIdTrackedAsync(cmd.CourseId, ct);
        if (course is null)
            return Result.NotFound($"Curso con id '{cmd.CourseId}' no encontrado.");

        if (!CourseAccess.CanManage(course, cmd.RequesterId, cmd.RequesterRole, cmd.RequesterCountryId))
            return Result.Forbidden(CourseAccess.ForbiddenMessage);

        if (!course.IsActive)
        {
            var lessons = await _lessons.GetByCourseAsync(cmd.CourseId, ct);
            if (lessons.Count == 0)
                return Result.Conflict(
                    "No puedes publicar un curso sin lecciones. Agrega al menos una lección primero.");

            course.Activate();
        }
        else
        {
            course.Deactivate();
        }

        await _courses.UpdateAsync(course, ct);

        return Result.Success();
    }
}