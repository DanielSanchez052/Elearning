using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Lessons.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Lessons;

public class UpdateLessonHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly UpdateLessonHandler _handler;

    public UpdateLessonHandlerTests() =>
        _handler = new UpdateLessonHandler(_coursesMock.Object, _lessonsMock.Object);

    [Fact]
    public async Task HandleAsync_LessonNotFound_ReturnsNotFound()
    {
        _lessonsMock
            .Setup(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Lesson?)null);

        var cmd = new UpdateLessonCommand(Guid.NewGuid(), "Nuevo título", "v2.mp4", true, Guid.NewGuid(), "instructor");
        var result = await _handler.HandleAsync(cmd);

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

        var cmd = new UpdateLessonCommand(lesson.Id, "Nuevo título", "v2.mp4", true, Guid.NewGuid(), "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_UpdatesLessonAndCallsUpdateAsync()
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

        var cmd = new UpdateLessonCommand(lesson.Id, "  Lección actualizada  ", "new.mp4", false, ownerId, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        Assert.Equal("Lección actualizada", lesson.Title);
        Assert.Equal("new.mp4", lesson.ContentUrl);
        Assert.False(lesson.IsRequired);
        _lessonsMock.Verify(r => r.UpdateAsync(lesson, default), Times.Once);
    }
}
