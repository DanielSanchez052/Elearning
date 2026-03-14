using ELearning.Application.Features.Auth.Commands;
using ELearning.Application.Features.Auth.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _validator = new();

    private static RegisterUserCommand ValidCmd() =>
        new("John Doe", "john@test.com", "Password1", CountryId: 1);

    // ── FullName ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyFullName_HasFullNameError(string? name)
    {
        var result = _validator.Validate(ValidCmd() with { FullName = name! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.FullName));
    }

    [Fact]
    public void Validate_FullNameTooShort_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { FullName = "A" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.FullName));
    }

    [Fact]
    public void Validate_FullNameTooLong_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { FullName = new string('A', 151) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.FullName));
    }

    // ── Email ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_HasEmailError(string? email)
    {
        var result = _validator.Validate(ValidCmd() with { Email = email! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public void Validate_InvalidEmailFormat_HasEmailError(string email)
    {
        var result = _validator.Validate(ValidCmd() with { Email = email });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Fact]
    public void Validate_EmailTooLong_HasEmailError()
    {
        var longEmail = new string('a', 192) + "@test.com"; // > 200 chars
        var result = _validator.Validate(ValidCmd() with { Email = longEmail });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    // ── Password ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyPassword_HasPasswordError(string? password)
    {
        var result = _validator.Validate(ValidCmd() with { Password = password! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public void Validate_PasswordTooShort_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { Password = "Abc1" }); // < 8 chars

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public void Validate_PasswordNoUppercase_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { Password = "password1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public void Validate_PasswordNoLowercase_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { Password = "PASSWORD1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public void Validate_PasswordNoDigit_HasError()
    {
        var result = _validator.Validate(ValidCmd() with { Password = "PasswordOnly" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    // ── CountryId ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_InvalidCountryId_HasCountryError(int countryId)
    {
        var result = _validator.Validate(ValidCmd() with { CountryId = countryId });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.CountryId));
    }

    // ── Valid ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(ValidCmd());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
