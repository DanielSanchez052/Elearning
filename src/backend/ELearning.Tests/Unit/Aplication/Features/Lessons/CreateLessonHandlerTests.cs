using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Lessons.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Lessons;

public class CreateLessonHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly CreateLessonHandler _handler;

    public CreateLessonHandlerTests() =>
        _handler = new CreateLessonHandler(_coursesMock.Object, _lessonsMock.Object);

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var cmd = new CreateLessonCommand(Guid.NewGuid(), "Lección", "video", "v.mp4", true, Guid.NewGuid(), "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorNotOwner_ReturnsForbidden()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);

        var cmd = new CreateLessonCommand(course.Id, "Lección", "video", "v.mp4", true, Guid.NewGuid(), "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesLessonWithMaxOrderPlusOne()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);
        Lesson? created = null;

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _lessonsMock
            .Setup(r => r.GetMaxOrderIndexAsync(course.Id, default))
            .ReturnsAsync(3);
        _lessonsMock
            .Setup(r => r.CreateAsync(It.IsAny<Lesson>(), default))
            .Callback<Lesson, CancellationToken>((l, _) => created = l)
            .Returns(Task.CompletedTask);

        var cmd = new CreateLessonCommand(course.Id, "  Intro  ", "VIDEO", "v.mp4", true, ownerId, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal("Intro", created!.Title);
        Assert.Equal(LessonType.Video, created.Type);
        Assert.Equal(4, created.OrderIndex);
        _lessonsMock.Verify(r => r.CreateAsync(It.IsAny<Lesson>(), default), Times.Once);
    }
}
