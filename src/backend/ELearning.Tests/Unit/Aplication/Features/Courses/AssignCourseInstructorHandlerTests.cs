using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Commands;
using ELearning.Application.Features.Courses.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class AssignCourseInstructorHandlerTests
{
    private const int CountryId = 57;
    private const int OtherCountryId = 52;

    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<ICourseInstructorNotifier> _notifierMock = new();
    private readonly AssignCourseInstructorHandler _handler;

    private static readonly Guid RequesterId = Guid.NewGuid();

    public AssignCourseInstructorHandlerTests() =>
        _handler = new AssignCourseInstructorHandler(_coursesMock.Object, _usersMock.Object, _notifierMock.Object);

    private Course SetupCourse(bool isGlobal = false, Guid? instructorId = null, int countryId = CountryId)
    {
        var course = Course.Create("Excel", null, null, Guid.NewGuid(), isGlobal, instructorId);
        if (!isGlobal)
            course.CourseCountries.Add(CourseCountry.Create(course.Id, countryId));
        _coursesMock.Setup(r => r.GetByIdTrackedAsync(course.Id, default)).ReturnsAsync(course);
        return course;
    }

    private User SetupUser(UserRole role = UserRole.Instructor, int countryId = CountryId)
    {
        var user = User.Create("Instructor", $"{Guid.NewGuid():N}@test.com", "hash", countryId, role);
        _usersMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        return user;
    }

    private void VerifyInstructorChanged(Course course, Guid? previous, Guid? next) =>
        _notifierMock.Verify(n => n.InstructorChangedAsync(course, previous, next, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyNoInstructorChange() =>
        _notifierMock.Verify(n => n.InstructorChangedAsync(
            It.IsAny<Course>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);

    private Task<Result> Assign(Course course, Guid? instructorId, string role = "admin") =>
        _handler.HandleAsync(new AssignCourseInstructorCommand(course.Id, instructorId, RequesterId, role, CountryId));

    [Fact]
    public async Task HandleAsync_AdminAssignsInstructorOfOwnCountry_AssignsAndNotifiesBoth()
    {
        var previous = SetupUser();
        var next = SetupUser();
        var course = SetupCourse(instructorId: previous.Id);

        var result = await Assign(course, next.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(next.Id, course.InstructorId);
        VerifyInstructorChanged(course, previous.Id, next.Id);
        _coursesMock.Verify(r => r.UpdateAsync(course, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RemovesInstructor_LeavesCourseUnassignedAndNotifiesPrevious()
    {
        var previous = SetupUser();
        var course = SetupCourse(instructorId: previous.Id);

        var result = await Assign(course, null);

        Assert.True(result.IsSuccess);
        Assert.Null(course.InstructorId);
        VerifyInstructorChanged(course, previous.Id, null);
    }

    [Fact]
    public async Task HandleAsync_SameInstructor_SucceedsWithoutChangesOrNotifications()
    {
        var instructor = SetupUser();
        var course = SetupCourse(instructorId: instructor.Id);

        var result = await Assign(course, instructor.Id);

        Assert.True(result.IsSuccess);
        VerifyNoInstructorChange();
        _coursesMock.Verify(r => r.UpdateAsync(It.IsAny<Course>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AdminPicksInstructorOfAnotherCountry_ReturnsNotFound()
    {
        var foreign = SetupUser(countryId: OtherCountryId);
        var course = SetupCourse();

        var result = await Assign(course, foreign.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Null(course.InstructorId);
    }

    [Fact]
    public async Task HandleAsync_TargetIsNotInstructor_ReturnsValidationFailure()
    {
        var student = SetupUser(UserRole.Student);
        var course = SetupCourse();

        var result = await Assign(course, student.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        VerifyNoInstructorChange();
    }

    [Fact]
    public async Task HandleAsync_UnknownUser_ReturnsNotFound()
    {
        var course = SetupCourse();

        var result = await Assign(course, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Theory]
    [InlineData(true, CountryId)]          // global: el admin no lo gestiona
    [InlineData(false, OtherCountryId)]    // curso de otro país
    public async Task HandleAsync_AdminOnCourseTheyDoNotManage_ReturnsForbidden(bool isGlobal, int courseCountry)
    {
        var instructor = SetupUser();
        var course = SetupCourse(isGlobal, countryId: courseCountry);

        var result = await Assign(course, instructor.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.Null(course.InstructorId);
    }

    [Fact]
    public async Task HandleAsync_SuperAdminAssignsAnyInstructorToGlobalCourse_Succeeds()
    {
        var foreign = SetupUser(countryId: OtherCountryId);
        var course = SetupCourse(isGlobal: true);

        var result = await Assign(course, foreign.Id, "superadmin");

        Assert.True(result.IsSuccess);
        Assert.Equal(foreign.Id, course.InstructorId);
    }

    [Fact]
    public async Task HandleAsync_InstructorCannotReassign_ReturnsForbidden()
    {
        var other = SetupUser();
        var course = SetupCourse(instructorId: RequesterId);

        var result = await Assign(course, other.Id, "instructor");

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.Equal(RequesterId, course.InstructorId);
    }

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock.Setup(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(
            new AssignCourseInstructorCommand(Guid.NewGuid(), null, RequesterId, "admin", CountryId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }
}
