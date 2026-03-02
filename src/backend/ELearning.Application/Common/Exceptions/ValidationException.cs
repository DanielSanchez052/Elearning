using ELearning.Application.Common.Validators;

namespace ELearning.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationResult ValidationResult { get; }

    public ValidationException(ValidationResult validationResult)
        : base("Validation failed")
    {
        ValidationResult = validationResult;
    }
}
