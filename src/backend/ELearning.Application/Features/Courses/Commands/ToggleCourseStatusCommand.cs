using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Commands;

public sealed record ToggleCourseStatusCommand(
    Guid CourseId,
    Guid RequesterId,
    string RequesterRole
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

        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);
        if (requesterRole == UserRole.Instructor && course.CreatedBy != cmd.RequesterId)
            return Result.Forbidden("Solo el instructor que creó el curso puede publicarlo.");

        // Al publicar, verificar que el curso tiene al menos una lección
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