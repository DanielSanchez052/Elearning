namespace ELearning.Application.Common.Validators;

public class ValidationError
{
    public string PropertyName { get; }
    public string ErrorMessage { get; }

    public ValidationError(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }
}

public class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

    public ValidationResult() { }

    public ValidationResult(IEnumerable<ValidationError> errors)
    {
        _errors.AddRange(errors);
    }

    public void AddError(string propertyName, string errorMessage) =>
        _errors.Add(new ValidationError(propertyName, errorMessage));

    public void AddErrors(ValidationResult other)
    {
        _errors.AddRange(other._errors);
    }
}
