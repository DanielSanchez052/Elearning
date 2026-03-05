
using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Commands;

public sealed record UpdateCourseCommand(
    Guid CourseId,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsGlobal,
    Guid RequesterId,   // extraído del JWT
    string RequesterRole  // extraído del JWT
) : ICommand;

public sealed class UpdateCourseHandler : ICommandHandler<UpdateCourseCommand>
{
    private readonly ICourseRepository _courses;

    public UpdateCourseHandler(ICourseRepository courses)
    {
        _courses = courses;
    }

    public async Task<Result> HandleAsync(
        UpdateCourseCommand cmd,
        CancellationToken ct = default)
    {
        var course = await _courses.GetByIdTrackedAsync(cmd.CourseId, ct);
        if (course is null)
            return Result.NotFound($"Curso con id '{cmd.CourseId}' no encontrado.");

        // Verificar permisos — solo el creador, admin o super_admin pueden editar
        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);
        if (requesterRole == UserRole.Instructor && course.CreatedBy != cmd.RequesterId)
            return Result.Forbidden("Solo el instructor que creó el curso puede editarlo.");

        course.Update(
            title: cmd.Title.Trim(),
            description: cmd.Description?.Trim(),
            thumbnailUrl: cmd.ThumbnailUrl,
            isGlobal: cmd.IsGlobal
        );

        await _courses.UpdateAsync(course, ct);

        return Result.Success();
    }
}
