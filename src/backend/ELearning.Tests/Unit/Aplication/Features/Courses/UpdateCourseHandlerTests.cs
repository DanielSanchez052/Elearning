using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class UpdateCourseHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly UpdateCourseHandler _handler;

    public UpdateCourseHandlerTests() =>
        _handler = new UpdateCourseHandler(_coursesMock.Object);

    private static Course BuildCourse(Guid createdBy, int countryId = 57, bool isGlobal = false)
    {
        var course = Course.Create("Título", "Desc", "thumb.png", createdBy, isGlobal, instructorId: createdBy);
        if (!isGlobal)
            course.CourseCountries.Add(CourseCountry.Create(course.Id, countryId));
        return course;
    }

    private static UpdateCourseCommand Valid(
        Guid courseId, Guid requesterId, string requesterRole = "instructor", int requesterCountryId = 57, bool isGlobal = false) =>
        new(
            CourseId: courseId,
            Title: "  Nuevo título  ",
            Description: "  Nueva descripción  ",
            ThumbnailUrl: "https://cdn.test/new.png",
            IsGlobal: isGlobal,
            RequesterId: requesterId,
            RequesterRole: requesterRole,
            RequesterCountryId: requesterCountryId);

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(Valid(Guid.NewGuid(), Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorNotOwner_ReturnsForbidden()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var course = BuildCourse(ownerId);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, requesterId, "instructor", 57));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorOwner_UpdatesCourseAndCallsUpdateAsync()
    {
        var ownerId = Guid.NewGuid();
        var course = BuildCourse(ownerId);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, ownerId, "instructor", 57));

        Assert.True(result.IsSuccess);
        Assert.Equal("Nuevo título", course.Title);
        Assert.Equal("Nueva descripción", course.Description);
        _coursesMock.Verify(r => r.UpdateAsync(course, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AdminNotOwner_UpdatesCourseSuccessfully()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var course = BuildCourse(ownerId);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, requesterId, "admin", 57));

        Assert.True(result.IsSuccess);
        _coursesMock.Verify(r => r.UpdateAsync(course, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AdminOfAnotherCountry_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid(), countryId: 57);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, Guid.NewGuid(), "admin", 52));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        _coursesMock.Verify(r => r.UpdateAsync(It.IsAny<Course>(), default), Times.Never);
    }

    [Theory]
    [InlineData("instructor")]
    [InlineData("admin")]
    public async Task HandleAsync_NonSuperAdminMakesCourseGlobal_ReturnsForbidden(string role)
    {
        var ownerId = Guid.NewGuid();
        var course = BuildCourse(ownerId);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, ownerId, role, 57, isGlobal: true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.False(course.IsGlobal);
    }

    [Fact]
    public async Task HandleAsync_AdminOnGlobalCourse_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid(), isGlobal: true);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, Guid.NewGuid(), "admin", 57, isGlobal: true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_AssignedInstructorEditsGlobalCourseKeepingScope_Succeeds()
    {
        var instructorId = Guid.NewGuid();
        var course = BuildCourse(instructorId, isGlobal: true);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, instructorId, "instructor", 57, isGlobal: true));

        Assert.True(result.IsSuccess);
        Assert.True(course.IsGlobal);
        Assert.Equal("Nuevo título", course.Title);
    }

    [Fact]
    public async Task HandleAsync_AssignedInstructorRemovesGlobalScope_ReturnsForbidden()
    {
        var instructorId = Guid.NewGuid();
        var course = BuildCourse(instructorId, isGlobal: true);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, instructorId, "instructor", 57, isGlobal: false));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.True(course.IsGlobal);
    }

    [Fact]
    public async Task HandleAsync_SuperAdminMakesCourseGlobal_Succeeds()
    {
        var course = BuildCourse(Guid.NewGuid(), countryId: 57);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(Valid(course.Id, Guid.NewGuid(), "superadmin", 1, isGlobal: true));

        Assert.True(result.IsSuccess);
        Assert.True(course.IsGlobal);
    }
}
