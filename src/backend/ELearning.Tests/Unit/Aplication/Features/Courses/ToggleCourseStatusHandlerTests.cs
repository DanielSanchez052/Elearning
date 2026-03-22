using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class ToggleCourseStatusHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly ToggleCourseStatusHandler _handler;

    public ToggleCourseStatusHandlerTests() =>
        _handler = new ToggleCourseStatusHandler(_coursesMock.Object, _lessonsMock.Object);

    private static Course BuildCourse(Guid ownerId, bool isActive)
    {
        var course = Course.Create("Título", "Desc", null, ownerId, isGlobal: false);
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

        var result = await _handler.HandleAsync(new ToggleCourseStatusCommand(Guid.NewGuid(), Guid.NewGuid(), "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorNotOwner_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid(), isActive: false);
        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new ToggleCourseStatusCommand(course.Id, Guid.NewGuid(), "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_PublishingWithoutLessons_ReturnsConflict()
    {
        var course = BuildCourse(Guid.NewGuid(), isActive: false);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(course.Id, default))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new ToggleCourseStatusCommand(course.Id, course.CreatedBy, "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_PublishingWithLessons_ActivatesCourse()
    {
        var course = BuildCourse(Guid.NewGuid(), isActive: false);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "video.mp4", 1, true);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(course.Id, default))
            .ReturnsAsync([lesson]);

        var result = await _handler.HandleAsync(new ToggleCourseStatusCommand(course.Id, course.CreatedBy, "instructor"));

        Assert.True(result.IsSuccess);
        Assert.True(course.IsActive);
        _coursesMock.Verify(r => r.UpdateAsync(course, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ActiveCourse_DeactivatesCourse()
    {
        var course = BuildCourse(Guid.NewGuid(), isActive: true);

        _coursesMock
            .Setup(r => r.GetByIdTrackedAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new ToggleCourseStatusCommand(course.Id, course.CreatedBy, "instructor"));

        Assert.True(result.IsSuccess);
        Assert.False(course.IsActive);
        _coursesMock.Verify(r => r.UpdateAsync(course, default), Times.Once);
    }
}
