using ELearning.Application.Features.Auth.Commands;
using ELearning.Application.Features.Auth.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_HasEmailError(string? email)
    {
        var result = _validator.Validate(new LoginCommand(email!, "pass"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyPassword_HasPasswordError(string? password)
    {
        var result = _validator.Validate(new LoginCommand("user@test.com", password!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new LoginCommand("user@test.com", "Password1"));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}