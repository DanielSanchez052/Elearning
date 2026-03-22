using ELearning.Application.Features.Auth.Commands;
using ELearning.Application.Features.Auth.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class ForgotPasswordValidatorTests
{
    private readonly ForgotPasswordValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_HasEmailError(string? email)
    {
        var result = _validator.Validate(new ForgotPasswordCommand(email!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
    }

    [Fact]
    public void Validate_ValidEmail_IsValid()
    {
        var result = _validator.Validate(new ForgotPasswordCommand("user@test.com"));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
