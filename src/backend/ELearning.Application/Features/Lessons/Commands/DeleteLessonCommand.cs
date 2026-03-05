using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Lessons.Commands;

public sealed record DeleteLessonCommand(
    Guid LessonId,
    Guid RequesterId,
    string RequesterRole
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
        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);
        if (requesterRole == UserRole.Instructor && course!.CreatedBy != cmd.RequesterId)
            return Result.Forbidden("Solo el instructor que creó el curso puede eliminar sus lecciones.");

        await _lessons.DeleteAsync(lesson, ct);

        return Result.Success();
    }
}