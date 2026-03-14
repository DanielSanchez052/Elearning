using ELearning.Application.Features.Users.Commands;
using ELearning.Application.Features.Users.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Users;

public class ChangeUserCountryValidatorTests
{
    private readonly ChangeUserCountryValidator _validator = new();

    private static ChangeUserCountryCommand Valid() =>
        new(Guid.NewGuid(), 1);

    [Fact]
    public void Validate_EmptyTargetUserId_HasTargetUserIdError()
    {
        var result = _validator.Validate(Valid() with { TargetUserId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeUserCountryCommand.TargetUserId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_NewCountryIdZeroOrNegative_HasCountryIdError(int countryId)
    {
        var result = _validator.Validate(Valid() with { NewCountryId = countryId });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeUserCountryCommand.NewCountryId));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_BothFieldsInvalid_HasTwoErrors()
    {
        var cmd = new ChangeUserCountryCommand(Guid.Empty, 0);
        var result = _validator.Validate(cmd);

        Assert.Equal(2, result.Errors.Count);
    }
}
