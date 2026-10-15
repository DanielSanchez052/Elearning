using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Commands;
using ELearning.Application.Features.Courses.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class CreateCourseHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<ICourseInstructorNotifier> _notifierMock = new();
    private readonly CreateCourseHandler _handler;

    public CreateCourseHandlerTests() =>
        _handler = new CreateCourseHandler(_coursesMock.Object, _usersMock.Object, _notifierMock.Object);

    private static CreateCourseCommand Valid(
        bool isGlobal = false, string creatorRole = "instructor", Guid? createdBy = null, Guid? instructorId = null) =>
        new(
            Title: "  Curso de C#  ",
            Description: "  Descripción test  ",
            ThumbnailUrl: "https://cdn.test/course.png",
            IsGlobal: isGlobal,
            CreatedBy: createdBy ?? Guid.NewGuid(),
            CreatorCountryId: 57,
            CreatorRole: creatorRole,
            InstructorId: instructorId);

    [Theory]
    [InlineData("instructor")]
    [InlineData("admin")]
    public async Task HandleAsync_NonSuperAdminCreatesGlobalCourse_ReturnsForbidden(string role)
    {
        var result = await _handler.HandleAsync(Valid(isGlobal: true, creatorRole: role));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        _coursesMock.Verify(r => r.CreateAsync(It.IsAny<Course>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithCourseId()
    {
        var result = await _handler.HandleAsync(Valid());

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task HandleAsync_NonGlobalCourse_AssignsCreatorCountry()
    {
        Guid createdCourseId = Guid.Empty;

        _coursesMock
            .Setup(r => r.CreateAsync(It.IsAny<Course>(), default))
            .Callback<Course, CancellationToken>((c, _) => createdCourseId = c.Id)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(Valid(isGlobal: false));

        _coursesMock.Verify(r => r.SetCourseCountriesAsync(createdCourseId, It.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 57), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GlobalCourse_DoesNotAssignCountries()
    {
        var result = await _handler.HandleAsync(Valid(isGlobal: true, creatorRole: "superadmin"));

        Assert.True(result.IsSuccess);

        _coursesMock.Verify(r => r.SetCourseCountriesAsync(It.IsAny<Guid>(), It.IsAny<List<int>>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_StoresTrimmedTitleAndDescription()
    {
        Course? created = null;

        _coursesMock
            .Setup(r => r.CreateAsync(It.IsAny<Course>(), default))
            .Callback<Course, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(Valid());

        Assert.NotNull(created);
        Assert.Equal("Curso de C#", created!.Title);
        Assert.Equal("Descripción test", created.Description);
    }

    // ── Instructor asignado ───────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_InstructorCreatesCourse_IsAssignedToThemWithoutNotification()
    {
        var instructorId = Guid.NewGuid();
        Course? created = null;
        _coursesMock
            .Setup(r => r.CreateAsync(It.IsAny<Course>(), default))
            .Callback<Course, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        // Aunque envíe otro instructor, queda asignado a sí mismo.
        var result = await _handler.HandleAsync(Valid(createdBy: instructorId, instructorId: Guid.NewGuid()));

        Assert.True(result.IsSuccess);
        Assert.Equal(instructorId, created!.InstructorId);
        _usersMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), default), Times.Never);
        _notifierMock.Verify(n => n.InstructorChangedAsync(
            It.IsAny<Course>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AdminCreatesWithoutInstructor_LeavesItUnassigned()
    {
        Course? created = null;
        _coursesMock
            .Setup(r => r.CreateAsync(It.IsAny<Course>(), default))
            .Callback<Course, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(Valid(creatorRole: "admin"));

        Assert.True(result.IsSuccess);
        Assert.Null(created!.InstructorId);
    }

    [Fact]
    public async Task HandleAsync_AdminCreatesWithInstructorOfOwnCountry_AssignsAndNotifies()
    {
        var instructor = User.Create("Instructor", "ins@test.com", "hash", 57, UserRole.Instructor);
        _usersMock.Setup(r => r.GetByIdAsync(instructor.Id, default)).ReturnsAsync(instructor);
        Course? created = null;
        _coursesMock
            .Setup(r => r.CreateAsync(It.IsAny<Course>(), default))
            .Callback<Course, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(Valid(creatorRole: "admin", instructorId: instructor.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(instructor.Id, created!.InstructorId);
        _notifierMock.Verify(n => n.InstructorChangedAsync(created, null, instructor.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(UserRole.Instructor, 52)] // instructor de otro país
    [InlineData(UserRole.Student, 57)]    // no es instructor
    public async Task HandleAsync_AdminCreatesWithIneligibleInstructor_ReturnsValidationFailure(UserRole role, int countryId)
    {
        var candidate = User.Create("Otro", "otro@test.com", "hash", countryId, role);
        _usersMock.Setup(r => r.GetByIdAsync(candidate.Id, default)).ReturnsAsync(candidate);

        var result = await _handler.HandleAsync(Valid(creatorRole: "admin", instructorId: candidate.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _coursesMock.Verify(r => r.CreateAsync(It.IsAny<Course>(), default), Times.Never);
    }
}
