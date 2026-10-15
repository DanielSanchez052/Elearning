using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Lessons.Commands;

public sealed record DeleteLessonCommand(
    Guid LessonId,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : ICommand;

public sealed class DeleteLessonHandler : ICommandHandler<DeleteLessonCommand>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public DeleteLessonHandler(
        ICourseRepository courses,
        ILessonRepository lessons)
    {
        _courses = courses;
        _lessons = lessons;
    }

    public async Task<Result> HandleAsync(
        DeleteLessonCommand cmd,
        CancellationToken ct = default)
    {
        var lesson = await _lessons.GetByIdTrackedAsync(cmd.LessonId, ct);
        if (lesson is null)
            return Result.NotFound($"Lección con id '{cmd.LessonId}' no encontrada.");

        var course = await _courses.GetByIdAsync(lesson.CourseId, ct);
        if (course is null || !CourseAccess.CanManage(course, cmd.RequesterId, cmd.RequesterRole, cmd.RequesterCountryId))
            return Result.Forbidden(CourseAccess.ForbiddenMessage);

        await _lessons.DeleteAsync(lesson, ct);

        return Result.Success();
    }
}