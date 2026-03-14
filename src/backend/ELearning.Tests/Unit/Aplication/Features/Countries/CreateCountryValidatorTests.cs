using ELearning.Application.Features.Countries.Commands;
using ELearning.Application.Features.Countries.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Countries;

public class CreateCountryValidatorTests
{
    private readonly CreateCountryValidator _validator = new();

    private static CreateCountryCommand Valid() => new("COL", "Colombia");

    // ── Code ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyCode_HasCodeError(string? code)
    {
        var result = _validator.Validate(Valid() with { Code = code! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCountryCommand.Code));
    }

    [Theory]
    [InlineData("CO")]    // 2 chars
    [InlineData("COLO")]  // 4 chars
    [InlineData("C")]     // 1 char
    public void Validate_CodeNot3Chars_HasCodeError(string code)
    {
        var result = _validator.Validate(Valid() with { Code = code });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCountryCommand.Code));
    }

    [Theory]
    [InlineData("C0L")]   // contiene número
    [InlineData("CO-")]   // contiene guión
    [InlineData("CO ")]   // contiene espacio
    public void Validate_CodeWithNonLetters_HasCodeError(string code)
    {
        var result = _validator.Validate(Valid() with { Code = code });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCountryCommand.Code));
    }

    [Theory]
    [InlineData("COL")]
    [InlineData("MEX")]
    [InlineData("col")]   // minúsculas también son letras
    public void Validate_ValidCode_NoCodeError(string code)
    {
        var result = _validator.Validate(Valid() with { Code = code });

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(CreateCountryCommand.Code));
    }

    // ── Name ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_HasNameError(string? name)
    {
        var result = _validator.Validate(Valid() with { Name = name! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCountryCommand.Name));
    }

    [Fact]
    public void Validate_NameTooLong_HasNameError()
    {
        var result = _validator.Validate(Valid() with { Name = new string('A', 101) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCountryCommand.Name));
    }

    [Fact]
    public void Validate_NameExactly100Chars_IsValid()
    {
        var result = _validator.Validate(Valid() with { Name = new string('A', 100) });

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(CreateCountryCommand.Name));
    }

    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
