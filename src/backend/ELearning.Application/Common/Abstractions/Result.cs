namespace ELearning.Application.Common.Abstractions;

/// <summary>
/// Representa el resultado de una operación que puede fallar de forma controlada.
/// Úsalo en lugar de lanzar excepciones para flujos de negocio esperados:
/// validaciones, conflictos, recursos no encontrados, acceso denegado.
///
/// Reserva las excepciones para errores del sistema (fallo de BD, timeout, etc.)
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    // ── Constructores de éxito ────────────────────────────────────────────────

    public static Result Success() =>
        new(true, null, ResultErrorType.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, null, ResultErrorType.None);

    // ── Constructores de fallo ────────────────────────────────────────────────

    /// <summary>Validación fallida (400 Bad Request)</summary>
    public static Result ValidationFailure(string error) =>
        new(false, error, ResultErrorType.Validation);

    public static Result<TValue> ValidationFailure<TValue>(string error) =>
        new(default, false, error, ResultErrorType.Validation);

    /// <summary>Recurso no encontrado (404 Not Found)</summary>
    public static Result NotFound(string error) =>
        new(false, error, ResultErrorType.NotFound);

    public static Result<TValue> NotFound<TValue>(string error) =>
        new(default, false, error, ResultErrorType.NotFound);

    /// <summary>Recurso ya existe (409 Conflict)</summary>
    public static Result Conflict(string error) =>
        new(false, error, ResultErrorType.Conflict);

    public static Result<TValue> Conflict<TValue>(string error) =>
        new(default, false, error, ResultErrorType.Conflict);

    /// <summary>Sin permisos (403 Forbidden)</summary>
    public static Result Forbidden(string error) =>
        new(false, error, ResultErrorType.Forbidden);

    public static Result<TValue> Forbidden<TValue>(string error) =>
        new(default, false, error, ResultErrorType.Forbidden);

    /// <summary>No autenticado (401 Unauthorized)</summary>
    public static Result Unauthorized(string error) =>
        new(false, error, ResultErrorType.Unauthorized);

    public static Result<TValue> Unauthorized<TValue>(string error) =>
        new(default, false, error, ResultErrorType.Unauthorized);
}

/// <summary>
/// Result tipado — para operaciones que retornan un valor en caso de éxito.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    internal Result(TValue? value, bool isSuccess, string? error, ResultErrorType errorType)
        : base(isSuccess, error, errorType)
    {
        _value = value;
    }

    // Conversión implícita desde TValue → Result<TValue> exitoso
    // Permite escribir: return user;  en lugar de: return Result.Success(user);
    public static implicit operator Result<TValue>(TValue value) =>
        Success(value);
}

/// <summary>
/// Tipo de error — permite al controlador decidir el status HTTP sin
/// acoplar la capa Application a ASP.NET Core.
/// </summary>
public enum ResultErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized
}