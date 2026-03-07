using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Enrollments.Commands;

namespace ELearning.Application.Features.Enrollments.Validators;

public sealed class EnrollInCourseValidator : IValidator<EnrollInCourseCommand>
{
    public ValidationResult Validate(EnrollInCourseCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.UserId == Guid.Empty)
            result.AddError(nameof(cmd.UserId), "El usuario es requerido.");

        if (cmd.CourseId == Guid.Empty)
            result.AddError(nameof(cmd.CourseId), "El curso es requerido.");

        return result;
    }
}
