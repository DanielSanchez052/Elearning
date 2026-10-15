using ELearning.Application.Common.Authorization;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Tests.Unit.Aplication.Common;

public class CourseAccessTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static Course BuildCourse(bool isGlobal = false, params int[] countryIds)
    {
        var course = Course.Create("Curso", null, null, OwnerId, isGlobal, instructorId: OwnerId);
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
    public void CanManage_AssignedInstructor_True() =>
        Assert.True(CourseAccess.CanManage(BuildCourse(false, 57), OwnerId, "instructor", 57));

    [Fact]
    public void CanManage_InstructorNotAssigned_False() =>
        Assert.False(CourseAccess.CanManage(BuildCourse(false, 57), Guid.NewGuid(), "instructor", 57));

    [Fact]
    public void CanManage_AssignedInstructor_GlobalCourse_True() =>
        Assert.True(CourseAccess.CanManage(BuildCourse(true), OwnerId, "instructor", 57));

    [Fact]
    public void CanManage_CreatorNoLongerAssigned_False()
    {
        var creatorId = Guid.NewGuid();
        var course = Course.Create("Curso", null, null, creatorId, isGlobal: false, instructorId: creatorId);
        course.AssignInstructor(Guid.NewGuid());

        Assert.False(CourseAccess.CanManage(course, creatorId, "instructor", 57));
    }

    [Fact]
    public void CanManage_UnassignedCourse_NoInstructorCanManageIt()
    {
        var course = Course.Create("Curso", null, null, Guid.NewGuid(), isGlobal: false);

        Assert.False(CourseAccess.CanManage(course, Guid.NewGuid(), "instructor", 57));
    }

    // ── Asignación de instructor ──────────────────────────────────────────────

    [Theory]
    [InlineData("admin", false, 57, true)]       // curso de su país
    [InlineData("admin", false, 52, false)]      // curso de otro país
    [InlineData("admin", true, 57, false)]       // global: el admin no lo gestiona
    [InlineData("superadmin", true, 52, true)]
    [InlineData("instructor", false, 57, false)] // aunque sea el asignado, no reasigna
    public void CanAssignInstructor_DependsOnRoleAndCourse(string role, bool isGlobal, int courseCountry, bool expected)
    {
        var course = isGlobal ? BuildCourse(true) : BuildCourse(false, courseCountry);

        Assert.Equal(expected, CourseAccess.CanAssignInstructor(course, OwnerId, role, 57));
    }

    [Theory]
    [InlineData(UserRole.Instructor, 57, "admin", true)]
    [InlineData(UserRole.Instructor, 52, "admin", false)]      // de otro país
    [InlineData(UserRole.Instructor, 52, "superadmin", true)]
    [InlineData(UserRole.Student, 57, "admin", false)]         // no es instructor
    [InlineData(UserRole.Admin, 57, "superadmin", false)]
    public void IsEligibleInstructor_RequiresInstructorRoleVisibleToRequester(
        UserRole candidateRole, int candidateCountry, string requesterRole, bool expected)
    {
        var candidate = User.Create("Candidato", "c@test.com", "hash", candidateCountry, candidateRole);

        Assert.Equal(expected, CourseAccess.IsEligibleInstructor(candidate, requesterRole, 57));
    }

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
