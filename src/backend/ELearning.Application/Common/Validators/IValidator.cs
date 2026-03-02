namespace ELearning.Application.Common.Validators;

public interface IValidator<T>
{
    ValidationResult Validate(T instance);
}
