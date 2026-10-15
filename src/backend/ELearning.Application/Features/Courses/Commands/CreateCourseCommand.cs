using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Application.Features.Courses.Services;
using ELearning.Domain.Constants;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Commands;

public sealed record CreateCourseCommand(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsGlobal,
    Guid CreatedBy,       // extraído del JWT en el controlador
    int CreatorCountryId, // extraído del JWT — el curso se asigna al país del creador
    string CreatorRole,   // extraído del JWT — solo el superadmin crea cursos globales
    Guid? InstructorId = null // solo admin/superadmin; un instructor queda asignado a sí mismo
) : ICommand<Guid>;

public sealed class CreateCourseHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly ICourseRepository _courses;
    private readonly IUserRepository _users;
    private readonly ICourseInstructorNotifier _instructorNotifier;

    public CreateCourseHandler(ICourseRepository courses, IUserRepository users, ICourseInstructorNotifier instructorNotifier)
    {
        _courses = courses;
        _users = users;
        _instructorNotifier = instructorNotifier;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateCourseCommand cmd,
        CancellationToken ct = default)
    {
        if (cmd.IsGlobal && !CourseAccess.IsSuperAdmin(cmd.CreatorRole))
            return Result.Forbidden<Guid>(CourseAccess.GlobalForbiddenMessage);

        var creatorIsInstructor = string.Equals(cmd.CreatorRole, RoleNames.Instructor, StringComparison.OrdinalIgnoreCase);
        var instructorId = creatorIsInstructor ? cmd.CreatedBy : cmd.InstructorId;

        if (!creatorIsInstructor && cmd.InstructorId is { } candidateId)
        {
            var candidate = await _users.GetByIdAsync(candidateId, ct);
            if (candidate is null || !CourseAccess.IsEligibleInstructor(candidate, cmd.CreatorRole, cmd.CreatorCountryId))
                return Result.ValidationFailure<Guid>("El instructor elegido no existe, no es instructor o no es de tu país.");
        }

        var course = Course.Create(
            title: cmd.Title.Trim(),
            description: cmd.Description?.Trim(),
            thumbnailUrl: cmd.ThumbnailUrl,
            createdBy: cmd.CreatedBy,
            isGlobal: cmd.IsGlobal,
            instructorId: instructorId
        );

        if (!creatorIsInstructor)
            await _instructorNotifier.InstructorChangedAsync(course, null, instructorId, ct);

        await _courses.CreateAsync(course, ct);

        if (!cmd.IsGlobal)
        {
            await _courses.SetCourseCountriesAsync(
                course.Id,
                [cmd.CreatorCountryId],
                ct);
        }

        return Result.Success(course.Id);
    }
}
