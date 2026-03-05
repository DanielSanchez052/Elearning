using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Lessons.Commands;

namespace ELearning.Application.Features.Lessons.Validators;

public sealed class ReorderLessonsValidator : IValidator<ReorderLessonsCommand>
{
    public ValidationResult Validate(ReorderLessonsCommand cmd)
    {
        var result = new ValidationResult();

        if (cmd.CourseId == Guid.Empty)
            result.AddError(nameof(cmd.CourseId), "El curso es requerido.");

        if (cmd.Orders is null || cmd.Orders.Count == 0)
            result.AddError(nameof(cmd.Orders), "El nuevo orden es requerido.");

        if (cmd.Orders?.Any(o => o.NewOrder < 1) == true)
            result.AddError(nameof(cmd.Orders), "Los índices de orden deben ser mayores a 0.");

        // Verificar que no hay índices duplicados
        var duplicates = cmd.Orders?
            .GroupBy(o => o.NewOrder)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicates?.Any() == true)
            result.AddError(nameof(cmd.Orders),
                "Los índices de orden no pueden repetirse.");

        return result;
    }
}
