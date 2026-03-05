using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Courses.Commands;

namespace ELearning.Application.Features.Courses.Validators;

public sealed class AssignCourseCountriesValidator : IValidator<AssignCourseCountriesCommand>
{
    public ValidationResult Validate(AssignCourseCountriesCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.CourseId == Guid.Empty)
            result.AddError(nameof(cmd.CourseId), "El curso es requerido.");

        if (cmd.CountryIds is null || cmd.CountryIds.Count == 0)
            result.AddError(nameof(cmd.CountryIds),
                "Debes asignar al menos un país al curso.");

        if (cmd.CountryIds?.Any(id => id <= 0) == true)
            result.AddError(nameof(cmd.CountryIds), "Todos los IDs de país deben ser válidos.");

        return result;
    }
}
