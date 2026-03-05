using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Lessons.Commands;

public sealed record ReorderLessonsCommand(
    Guid CourseId,
    List<LessonOrderItem> Orders,   // lista completa del nuevo orden
    Guid RequesterId,
    string RequesterRole
) : ICommand;

public sealed record LessonOrderItem(Guid LessonId, int NewOrder);

public sealed class ReorderLessonsHandler : ICommandHandler<ReorderLessonsCommand>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public ReorderLessonsHandler(
        ICourseRepository courses,
        ILessonRepository lessons)
    {
        _courses = courses;
        _lessons = lessons;
    }

    public async Task<Result> HandleAsync(
        ReorderLessonsCommand cmd,
        CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(cmd.CourseId, ct);
        if (course is null)
            return Result.NotFound($"Curso con id '{cmd.CourseId}' no encontrado.");

        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);
        if (requesterRole == UserRole.Instructor && course.CreatedBy != cmd.RequesterId)
            return Result.Forbidden("Solo el instructor que creó el curso puede reordenar sus lecciones.");

        // Verificar que todas las lecciones pertenecen al curso
        var currentLessons = await _lessons.GetByCourseAsync(cmd.CourseId, ct);
        var currentIds = currentLessons.Select(l => l.Id).ToHashSet();

        var invalidIds = cmd.Orders
            .Where(o => !currentIds.Contains(o.LessonId))
            .ToList();

        if (invalidIds.Any())
            return Result.Conflict(
                $"Las siguientes lecciones no pertenecen al curso: " +
                $"{string.Join(", ", invalidIds.Select(o => o.LessonId))}.");

        var orders = cmd.Orders.Select(o => (o.LessonId, o.NewOrder));
        await _lessons.UpdateOrdersAsync(orders, ct);

        return Result.Success();
    }
}