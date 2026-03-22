using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Lessons.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Lessons;

public class DeleteLessonHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly DeleteLessonHandler _handler;

    public DeleteLessonHandlerTests() =>
        _handler = new DeleteLessonHandler(_coursesMock.Object, _lessonsMock.Object);

    [Fact]
    public async Task HandleAsync_LessonNotFound_ReturnsNotFound()
    {
        _lessonsMock
            .Setup(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Lesson?)null);

        var result = await _handler.HandleAsync(new DeleteLessonCommand(Guid.NewGuid(), Guid.NewGuid(), "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorNotOwner_ReturnsForbidden()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1);

        _lessonsMock
            .Setup(r => r.GetByIdTrackedAsync(lesson.Id, default))
            .ReturnsAsync(lesson);
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new DeleteLessonCommand(lesson.Id, Guid.NewGuid(), "instructor"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CallsDeleteAsync()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1);

        _lessonsMock
            .Setup(r => r.GetByIdTrackedAsync(lesson.Id, default))
            .ReturnsAsync(lesson);
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new DeleteLessonCommand(lesson.Id, ownerId, "instructor"));

        Assert.True(result.IsSuccess);
        _lessonsMock.Verify(r => r.DeleteAsync(lesson, default), Times.Once);
    }
}
