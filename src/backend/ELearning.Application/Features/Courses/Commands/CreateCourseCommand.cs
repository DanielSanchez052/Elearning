using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Application.Features.Courses.Commands;

public sealed record CreateCourseCommand(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsGlobal,
    Guid CreatedBy,      // extraído del JWT en el controlador
    int CreatorCountryId // extraído del JWT — instructor solo crea en su país
) : ICommand<Guid>;

public sealed class CreateCourseHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly ICourseRepository _courses;

    public CreateCourseHandler(ICourseRepository courses)
    {
        _courses = courses;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateCourseCommand cmd,
        CancellationToken ct = default)
    {
        var course = Course.Create(
            title: cmd.Title.Trim(),
            description: cmd.Description?.Trim(),
            thumbnailUrl: cmd.ThumbnailUrl,
            createdBy: cmd.CreatedBy,
            isGlobal: cmd.IsGlobal
        );

        await _courses.CreateAsync(course, ct);

        // Si no es global, asignar automáticamente al país del instructor
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