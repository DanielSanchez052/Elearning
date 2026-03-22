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

    private static Course BuildCourse(Guid createdBy)
    {
        return Course.Create("Título", "Desc", "thumb.png", createdBy, isGlobal: false);
    }

    private static UpdateCourseCommand Valid(Guid courseId, Guid requesterId, string requesterRole = "instructor") =>
        new(
            CourseId: courseId,
            Title: "  Nuevo título  ",
            Description: "  Nueva descripción  ",
            ThumbnailUrl: "https://cdn.test/new.png",
            IsGlobal: true,
            RequesterId: requesterId,
            RequesterRole: requesterRole);

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

        var result = await _handler.HandleAsync(Valid(course.Id, requesterId, "instructor"));

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

        var result = await _handler.HandleAsync(Valid(course.Id, ownerId, "instructor"));

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

        var result = await _handler.HandleAsync(Valid(course.Id, requesterId, "admin"));

        Assert.True(result.IsSuccess);
        _coursesMock.Verify(r => r.UpdateAsync(course, default), Times.Once);
    }
}
