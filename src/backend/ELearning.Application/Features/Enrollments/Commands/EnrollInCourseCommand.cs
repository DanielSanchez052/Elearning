using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Exceptions;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Enrollments.Commands;

public record EnrollInCourseCommand(Guid UserId, Guid CourseId) : ICommand<Guid>;

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

        if(course is null)
            return Result.NotFound<Guid>($"Curso con id '{command.CourseId}' no encontrado.");

        if (!course.IsActive)
            return Result.Conflict<Guid>("No es posible inscribirse en un curso inactivo.");

        var alreadyEnrolled = await _enrollments.ExistsAsync(command.UserId, command.CourseId, ct);
        if (alreadyEnrolled)
            return Result.Conflict<Guid>("El usuario ya está inscrito en este curso.");

        var enrollment = CourseEnrollment.Create(command.UserId, command.CourseId);

        await _enrollments.AddAsync(enrollment, ct);
        await _enrollments.SaveChangesAsync(ct);

        return enrollment.Id;
    }
}
