using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Lessons.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Lessons;

public class ReorderLessonsHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly ReorderLessonsHandler _handler;

    public ReorderLessonsHandlerTests() =>
        _handler = new ReorderLessonsHandler(_coursesMock.Object, _lessonsMock.Object);

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var cmd = new ReorderLessonsCommand(Guid.NewGuid(), [new LessonOrderItem(Guid.NewGuid(), 1)], Guid.NewGuid(), "instructor");
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

        var cmd = new ReorderLessonsCommand(course.Id, [new LessonOrderItem(Guid.NewGuid(), 1)], Guid.NewGuid(), "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_OrdersContainLessonOutsideCourse_ReturnsConflict()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);
        var lesson = Lesson.Create(course.Id, "L1", LessonType.Video, "v.mp4", 1);
        var invalidLessonId = Guid.NewGuid();

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(course.Id, default))
            .ReturnsAsync([lesson]);

        var cmd = new ReorderLessonsCommand(course.Id, [new LessonOrderItem(invalidLessonId, 1)], ownerId, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_CallsUpdateOrdersAsync()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);
        var lesson1 = Lesson.Create(course.Id, "L1", LessonType.Video, "v1.mp4", 1);
        var lesson2 = Lesson.Create(course.Id, "L2", LessonType.Pdf, "p.pdf", 2);

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(course.Id, default))
            .ReturnsAsync([lesson1, lesson2]);

        IEnumerable<(Guid LessonId, int NewOrder)>? captured = null;
        _lessonsMock
            .Setup(r => r.UpdateOrdersAsync(It.IsAny<IEnumerable<(Guid LessonId, int NewOrder)>>(), default))
            .Callback<IEnumerable<(Guid LessonId, int NewOrder)>, CancellationToken>((orders, _) => captured = orders)
            .Returns(Task.CompletedTask);

        var cmd = new ReorderLessonsCommand(
            course.Id,
            [new LessonOrderItem(lesson1.Id, 2), new LessonOrderItem(lesson2.Id, 1)],
            ownerId,
            "instructor");

        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Contains(captured!, x => x.LessonId == lesson1.Id && x.NewOrder == 2);
        Assert.Contains(captured!, x => x.LessonId == lesson2.Id && x.NewOrder == 1);
    }
}
