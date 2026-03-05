using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Courses.Commands;

namespace ELearning.Application.Features.Courses.Validators;

public sealed class UpdateCourseValidator : IValidator<UpdateCourseCommand>
{
    public ValidationResult Validate(UpdateCourseCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.CourseId == Guid.Empty)
            result.AddError(nameof(cmd.CourseId), "El curso es requerido.");

        if (string.IsNullOrWhiteSpace(cmd.Title))
            result.AddError(nameof(cmd.Title), "El título del curso es requerido.");
        else if (cmd.Title.Length > 200)
            result.AddError(nameof(cmd.Title), "El título no puede superar 200 caracteres.");

        if (cmd.Description?.Length > 2000)
            result.AddError(nameof(cmd.Description), "La descripción no puede superar 2000 caracteres.");

        return result;
    }
}
