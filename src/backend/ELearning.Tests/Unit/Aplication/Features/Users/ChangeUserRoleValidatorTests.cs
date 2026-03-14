using ELearning.Application.Features.Users.Commands;
using ELearning.Application.Features.Users.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Users;

public class ChangeUserRoleValidatorTests
{
    private readonly ChangeUserRoleValidator _validator = new();

    private static ChangeUserRoleCommand Valid() =>
        new(Guid.NewGuid(), "instructor", Guid.NewGuid(), "super_admin");

    // ── TargetUserId ──────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyTargetUserId_HasTargetUserIdError()
    {
        var result = _validator.Validate(Valid() with { TargetUserId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeUserRoleCommand.TargetUserId));
    }

    // ── NewRole ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyNewRole_HasNewRoleError(string? newRole)
    {
        var result = _validator.Validate(Valid() with { NewRole = newRole! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeUserRoleCommand.NewRole));
    }

    [Theory]
    [InlineData("moderator")]
    [InlineData("owner")]
    [InlineData("god")]
    [InlineData("INVALID_ROLE")]
    public void Validate_InvalidRole_HasNewRoleError(string newRole)
    {
        var result = _validator.Validate(Valid() with { NewRole = newRole });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeUserRoleCommand.NewRole));
    }

    [Theory]
    [InlineData("student")]
    [InlineData("instructor")]
    [InlineData("admin")]
    [InlineData("superadmin")]
    public void Validate_ValidRoleLowercase_IsValid(string newRole)
    {
        var result = _validator.Validate(Valid() with { NewRole = newRole });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Student")]
    [InlineData("INSTRUCTOR")]
    [InlineData("Admin")]
    [InlineData("SUPERADMIN")]
    public void Validate_ValidRoleAnyCase_IsValid(string newRole)
    {
        // El validator hace .ToLowerInvariant() antes de Contains
        var result = _validator.Validate(Valid() with { NewRole = newRole });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyNewRole_EarlyReturn_NoRoleFormatError()
    {
        // Cuando NewRole está vacío hace early return (no añade error de formato)
        var result = _validator.Validate(Valid() with { NewRole = "" });

        Assert.Single(result.Errors);
    }

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // ── Múltiples errores simultáneos ──────────────────────────────────────

    [Fact]
    public void Validate_EmptyTargetIdAndInvalidRole_HasTwoErrors()
    {
        var cmd = new ChangeUserRoleCommand(Guid.Empty, "invalid_role", Guid.NewGuid(), "super_admin");
        var result = _validator.Validate(cmd);

        Assert.Equal(2, result.Errors.Count);
    }
}
