using ELearning.Application.Features.Auth.Commands;
using ELearning.Application.Features.Auth.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class ResetPasswordValidatorTests
{
    private readonly ResetPasswordValidator _validator = new();

    private static ResetPasswordCommand ValidCmd() =>
        new("valid-token", "NewPassword1", "NewPassword1");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyToken_HasTokenError(string? token)
    {
        var result = _validator.Validate(ValidCmd() with { Token = token! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.Token));
    }

    [Fact]
    public void Validate_PasswordTooShort_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { NewPassword = "Ab1", ConfirmPassword = "Ab1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswordNoUppercase_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { NewPassword = "password1", ConfirmPassword = "password1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswordNoLowercase_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { NewPassword = "PASSWORD1", ConfirmPassword = "PASSWORD1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswordNoDigit_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { NewPassword = "PasswordOnly", ConfirmPassword = "PasswordOnly" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswordsDoNotMatch_HasConfirmError()
    {
        var result = _validator.Validate(ValidCmd() with { ConfirmPassword = "DifferentPassword1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.ConfirmPassword));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(ValidCmd());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
