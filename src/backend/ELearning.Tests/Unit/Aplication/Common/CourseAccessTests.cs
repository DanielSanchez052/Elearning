using ELearning.Application.Common.Authorization;
using ELearning.Domain.Entities;

namespace ELearning.Tests.Unit.Aplication.Common;

public class CourseAccessTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static Course BuildCourse(bool isGlobal = false, params int[] countryIds)
    {
        var course = Course.Create("Curso", null, null, OwnerId, isGlobal);
        foreach (var countryId in countryIds)
            course.CourseCountries.Add(CourseCountry.Create(course.Id, countryId));
        return course;
    }

    // ── CanManage ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CanManage_SuperAdmin_AnyCourse(bool isGlobal) =>
        Assert.True(CourseAccess.CanManage(BuildCourse(isGlobal, 52), Guid.NewGuid(), "superadmin", 57));

    [Theory]
    [InlineData(new[] { 57 }, true)]
    [InlineData(new[] { 52, 57 }, true)]   // compartido con otro país: basta con incluir el suyo
    [InlineData(new[] { 52 }, false)]
    [InlineData(new int[0], false)]
    public void CanManage_Admin_DependsOnCourseCountries(int[] countryIds, bool expected) =>
        Assert.Equal(expected, CourseAccess.CanManage(BuildCourse(false, countryIds), Guid.NewGuid(), "admin", 57));

    [Fact]
    public void CanManage_Admin_GlobalCourse_False() =>
        Assert.False(CourseAccess.CanManage(BuildCourse(true, 57), Guid.NewGuid(), "admin", 57));

    [Fact]
    public void CanManage_InstructorOwner_True() =>
        Assert.True(CourseAccess.CanManage(BuildCourse(false, 57), OwnerId, "instructor", 57));

    [Fact]
    public void CanManage_InstructorNotOwner_False() =>
        Assert.False(CourseAccess.CanManage(BuildCourse(false, 57), Guid.NewGuid(), "instructor", 57));

    [Fact]
    public void CanManage_InstructorOwner_GlobalCourse_False() =>
        Assert.False(CourseAccess.CanManage(BuildCourse(true), OwnerId, "instructor", 57));

    [Theory]
    [InlineData("student")]
    [InlineData("1")]
    [InlineData("super_admin")]
    [InlineData("")]
    public void CanManage_UnknownOrNonStaffRole_False(string role) =>
        Assert.False(CourseAccess.CanManage(BuildCourse(false, 57), OwnerId, role, 57));

    [Fact]
    public void CanManage_RoleIsCaseInsensitive() =>
        Assert.True(CourseAccess.CanManage(BuildCourse(false, 57), Guid.NewGuid(), "SuperAdmin", 1));

    // ── CanAssignCountries ────────────────────────────────────────────────────

    [Theory]
    [InlineData(new[] { 52 }, new[] { 52, 57 }, true)]   // añade el suyo
    [InlineData(new[] { 52, 57 }, new[] { 52 }, true)]   // quita el suyo
    [InlineData(new[] { 57 }, new[] { 57 }, true)]       // sin cambios
    [InlineData(new[] { 57 }, new[] { 57, 52 }, false)]  // añade otro
    [InlineData(new[] { 52, 57 }, new[] { 57 }, false)]  // quita otro
    public void CanAssignCountries_NonSuperAdmin_OnlyOwnCountry(int[] current, int[] requested, bool expected) =>
        Assert.Equal(expected, CourseAccess.CanAssignCountries(BuildCourse(false, current), requested, "admin", 57));

    [Fact]
    public void CanAssignCountries_SuperAdmin_AnyChange() =>
        Assert.True(CourseAccess.CanAssignCountries(BuildCourse(false, 57), [1, 2, 3], "superadmin", 57));
}
