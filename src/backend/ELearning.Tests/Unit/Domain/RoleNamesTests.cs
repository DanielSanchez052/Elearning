using ELearning.Domain.Constants;
using ELearning.Domain.Enums;

namespace ELearning.Tests.Unit.Domain;

public class RoleNamesTests
{
    [Theory]
    [InlineData(UserRole.Student, "student")]
    [InlineData(UserRole.Instructor, "instructor")]
    [InlineData(UserRole.Admin, "admin")]
    [InlineData(UserRole.SuperAdmin, "superadmin")]
    public void ToRoleName_MatchesTheNameUsedInTokensAndAuthorize(UserRole role, string expected) =>
        Assert.Equal(expected, role.ToRoleName());

    [Fact]
    public void All_CoversEveryUserRole() =>
        Assert.Equal(
            Enum.GetValues<UserRole>().Select(r => r.ToRoleName()).OrderBy(n => n),
            RoleNames.All.OrderBy(n => n));

    [Theory]
    [InlineData("superadmin", UserRole.SuperAdmin)]
    [InlineData("SuperAdmin", UserRole.SuperAdmin)]
    [InlineData("INSTRUCTOR", UserRole.Instructor)]
    public void TryParse_AcceptsRoleNamesIgnoringCase(string name, UserRole expected)
    {
        Assert.True(RoleNames.TryParse(name, out var role));
        Assert.Equal(expected, role);
    }

    [Theory]
    [InlineData("1")]            // Enum.TryParse lo aceptaría como Admin
    [InlineData("super_admin")]
    [InlineData("teacher")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_RejectsAnythingElse(string? name) =>
        Assert.False(RoleNames.TryParse(name, out _));

    [Fact]
    public void Combinations_ListTheExpectedRoles()
    {
        Assert.Equal("admin,superadmin", RoleNames.Admins);
        Assert.Equal("instructor,admin,superadmin", RoleNames.Staff);
    }
}
