using ELearning.Application.Common.Validators;
using ELearning.Application.Features.Auth.Commands;

namespace ELearning.Application.Features.Auth.Validators;

public sealed class RegisterUserValidator : IValidator<RegisterUserCommand>
{
    public ValidationResult Validate(RegisterUserCommand cmd)
    {
        var result = new ValidationResult();

        ValidateFullName(cmd.FullName, result);
        ValidateEmail(cmd.Email, result);
        ValidatePassword(cmd.Password, result);
        ValidateCountry(cmd.CountryId, result);

        return result;
    }

    // ── Full Name ─────────────────────────────────────────────────────────────

    private static void ValidateFullName(string fullName, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            result.AddError(nameof(RegisterUserCommand.FullName), "El nombre completo es requerido.");
            return;
        }

        if (fullName.Trim().Length < 2)
            result.AddError(nameof(RegisterUserCommand.FullName), "El nombre debe tener al menos 2 caracteres.");

        if (fullName.Length > 150)
            result.AddError(nameof(RegisterUserCommand.FullName), "El nombre no puede superar 150 caracteres.");
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    private static void ValidateEmail(string email, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            result.AddError(nameof(RegisterUserCommand.Email), "El email es requerido.");
            return; // no tiene sentido validar formato si está vacío
        }

        if (email.Length > 200)
        {
            result.AddError(nameof(RegisterUserCommand.Email), "El email no puede superar 200 caracteres.");
            return;
        }

        if (!IsValidEmail(email))
            result.AddError(nameof(RegisterUserCommand.Email), "El email no tiene un formato válido.");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Password ──────────────────────────────────────────────────────────────

    private static void ValidatePassword(string password, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            result.AddError(nameof(RegisterUserCommand.Password), "La contraseña es requerida.");
            return; // no tiene sentido validar reglas si está vacío
        }

        // Validaciones separadas para que el frontend pueda mostrar exactamente qué falta
        if (password.Length < 8)
            result.AddError(nameof(RegisterUserCommand.Password), "Debe tener al menos 8 caracteres.");

        if (!password.Any(char.IsUpper))
            result.AddError(nameof(RegisterUserCommand.Password), "Debe contener al menos una letra mayúscula.");

        if (!password.Any(char.IsLower))
            result.AddError(nameof(RegisterUserCommand.Password), "Debe contener al menos una letra minúscula.");

        if (!password.Any(char.IsDigit))
            result.AddError(nameof(RegisterUserCommand.Password), "Debe contener al menos un número.");
    }

    // ── Country ───────────────────────────────────────────────────────────────

    private static void ValidateCountry(int countryId, ValidationResult result)
    {
        if (countryId <= 0)
            result.AddError(nameof(RegisterUserCommand.CountryId), "El país es requerido.");
    }
}