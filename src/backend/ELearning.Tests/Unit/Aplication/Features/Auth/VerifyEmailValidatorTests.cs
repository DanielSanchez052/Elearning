using ELearning.Application.Features.Auth.Commands;
using ELearning.Application.Features.Auth.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Auth;

public class VerifyEmailValidatorTests
{
    private readonly VerifyEmailValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyToken_HasError(string? token)
    {
        var result = _validator.Validate(new VerifyEmailCommand(token!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VerifyEmailCommand.Token));
    }

    [Fact]
    public void Validate_ValidToken_IsValid()
    {
        var result = _validator.Validate(new VerifyEmailCommand("abc123token"));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
