using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Commands;

public sealed record AssignCourseCountriesCommand(
    Guid CourseId,
    List<int> CountryIds,  // lista completa — reemplaza la asignación actual
    Guid RequesterId,
    string RequesterRole
) : ICommand;

public sealed class AssignCourseCountriesHandler : ICommandHandler<AssignCourseCountriesCommand>
{
    private readonly ICourseRepository _courses;
    private readonly ICountryRepository _countries;

    public AssignCourseCountriesHandler(
        ICourseRepository courses,
        ICountryRepository countries)
    {
        _courses = courses;
        _countries = countries;
    }

    public async Task<Result> HandleAsync(
        AssignCourseCountriesCommand cmd,
        CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(cmd.CourseId, ct);
        if (course is null)
            return Result.NotFound($"Curso con id '{cmd.CourseId}' no encontrado.");

        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);
        if (requesterRole == UserRole.Instructor && course.CreatedBy != cmd.RequesterId)
            return Result.Forbidden("Solo el instructor que creó el curso puede asignar países.");

        // Cursos globales no necesitan asignación de países
        if (course.IsGlobal)
            return Result.Conflict(
                "El curso es global y está disponible en todos los países. " +
                "Cambia IsGlobal a false antes de asignar países específicos.");

        // Verificar que todos los países existen y están activos
        foreach (var countryId in cmd.CountryIds.Distinct())
        {
            var country = await _countries.GetByIdAsync(countryId, ct);
            if (country is null)
                return Result.NotFound($"País con id '{countryId}' no encontrado.");
            if (!country.IsActive)
                return Result.Conflict(
                    $"El país '{country.Name}' está desactivado y no puede asignarse.");
        }

        await _courses.SetCourseCountriesAsync(
            cmd.CourseId,
            cmd.CountryIds.Distinct().ToList(),
            ct);

        return Result.Success();
    }
}