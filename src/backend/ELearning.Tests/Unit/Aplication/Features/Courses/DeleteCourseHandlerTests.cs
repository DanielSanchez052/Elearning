using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class DeleteCourseHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly DeleteCourseHandler _handler;

    public DeleteCourseHandlerTests() =>
        _handler = new DeleteCourseHandler(_coursesMock.Object);

    private static Course BuildCourse(Guid createdBy, bool isActive = false)
    {
        var course = Course.Create("Título", "Desc", null, createdBy, isGlobal: false);
        if (isActive)
            course.Activate();
        return course;
    }

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(new DeleteCourseCommand(Guid.NewGuid(), Guid.NewGuid(), "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorNotOwner_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid());

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new DeleteCourseCommand(course.Id, Guid.NewGuid(), "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ActiveCourseWithEnrollments_ReturnsConflict()
    {
        var course = BuildCourse(Guid.NewGuid(), isActive: true);
        course.Enrollments.Add(CourseEnrollment.Create(Guid.NewGuid(), course.Id));

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new DeleteCourseCommand(course.Id, course.CreatedBy, "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_DeactivatesCourseAndCallsUpdateAsync()
    {
        var course = BuildCourse(Guid.NewGuid(), isActive: true);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new DeleteCourseCommand(course.Id, course.CreatedBy, "instructor"));

        Assert.True(result.IsSuccess);
        Assert.False(course.IsActive);
        _coursesMock.Verify(r => r.UpdateAsync(course, default), Times.Once);
    }
}
