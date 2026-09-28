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
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly UpdateLessonHandler _handler;

    public UpdateLessonHandlerTests() =>
        _handler = new UpdateLessonHandler(_coursesMock.Object, _lessonsMock.Object, _quizzesMock.Object);

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

    [Fact]
    public async Task HandleAsync_QuizLessonWithoutQuestionsMarkedRequired_ReturnsValidationFailureAndDoesNotUpdate()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);
        var lesson = Lesson.Create(course.Id, "Quiz Capítulo 1", LessonType.Quiz, null, 1, isRequired: false);

        _lessonsMock
            .Setup(r => r.GetByIdTrackedAsync(lesson.Id, default))
            .ReturnsAsync(lesson);
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync(new List<QuizQuestion>());

        var cmd = new UpdateLessonCommand(lesson.Id, "Quiz Capítulo 1", null, true, ownerId, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _lessonsMock.Verify(r => r.UpdateAsync(It.IsAny<Lesson>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_QuizLessonWithQuestionsMarkedRequired_UpdatesSuccessfully()
    {
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Curso", "Desc", null, ownerId, isGlobal: false);
        var lesson = Lesson.Create(course.Id, "Quiz Capítulo 1", LessonType.Quiz, null, 1, isRequired: false);
        var question = QuizQuestion.CreatePerLesson(lesson.Id, "Pregunta", 60m, 3, 1, true);

        _lessonsMock
            .Setup(r => r.GetByIdTrackedAsync(lesson.Id, default))
            .ReturnsAsync(lesson);
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync(new List<QuizQuestion> { question });

        var cmd = new UpdateLessonCommand(lesson.Id, "Quiz Capítulo 1", null, true, ownerId, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        Assert.True(lesson.IsRequired);
        _lessonsMock.Verify(r => r.UpdateAsync(lesson, default), Times.Once);
    }
}
