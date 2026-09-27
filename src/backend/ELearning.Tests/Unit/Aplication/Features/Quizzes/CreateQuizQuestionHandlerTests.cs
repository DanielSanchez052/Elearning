using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class CreateQuizQuestionHandlerTests
{
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly CreateQuizQuestionHandler _handler;

    public CreateQuizQuestionHandlerTests() =>
        _handler = new CreateQuizQuestionHandler(_lessonsMock.Object, _coursesMock.Object, _quizzesMock.Object);

    [Fact]
    public async Task HandleAsync_PerLesson_LessonNotFound_ReturnsNotFound()
    {
        _lessonsMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Lesson?)null);

        var result = await _handler.HandleAsync(new CreateQuizQuestionCommand(
            Guid.NewGuid(), null, (int)QuizType.PerLesson, "Pregunta", 60m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CourseExam_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(new CreateQuizQuestionCommand(
            null, Guid.NewGuid(), (int)QuizType.CourseExam, "Pregunta", 70m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidPerLesson_CreatesAndSavesQuestion()
    {
        var lesson = Lesson.Create(Guid.NewGuid(), "Leccion", LessonType.Video, null, 1);
        QuizQuestion? created = null;

        _lessonsMock
            .Setup(r => r.GetByIdAsync(lesson.Id, default))
            .ReturnsAsync(lesson);
        _quizzesMock
            .Setup(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default))
            .Callback<QuizQuestion, CancellationToken>((q, _) => created = q)
            .ReturnsAsync((QuizQuestion q, CancellationToken _) => q);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new CreateQuizQuestionCommand(
            lesson.Id, null, (int)QuizType.PerLesson, "Pregunta", 60m, 3, true));

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal(QuizType.PerLesson, created!.Type);
        Assert.Equal(lesson.Id, created.LessonId);
        Assert.Equal("Pregunta", created.QuestionText);
        Assert.Equal(result.Value, created.Id);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidCourseExam_CreatesAndSavesQuestion()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        QuizQuestion? created = null;

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _quizzesMock
            .Setup(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default))
            .Callback<QuizQuestion, CancellationToken>((q, _) => created = q)
            .ReturnsAsync((QuizQuestion q, CancellationToken _) => q);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new CreateQuizQuestionCommand(
            null, course.Id, (int)QuizType.CourseExam, "Pregunta de examen", 70m, 2, false));

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal(QuizType.CourseExam, created!.Type);
        Assert.Equal(course.Id, created.CourseId);
        Assert.False(created.IsRequired);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_InvalidQuizType_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new CreateQuizQuestionCommand(
            Guid.NewGuid(), Guid.NewGuid(), 99, "Pregunta", 60m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    // The validator only checks that exactly one FK is present, not that it matches Type —
    // so Type=PerLesson with LessonId=null but a (mismatched) CourseId provided reaches the
    // handler. LessonId == Guid.Empty doesn't catch null, so cmd.LessonId.Value used to throw.
    [Fact]
    public async Task HandleAsync_PerLesson_NullLessonId_ReturnsValidationFailureInsteadOfThrowing()
    {
        var result = await _handler.HandleAsync(new CreateQuizQuestionCommand(
            null, Guid.NewGuid(), (int)QuizType.PerLesson, "Pregunta", 60m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _lessonsMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), default), Times.Never);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CourseExam_NullCourseId_ReturnsValidationFailureInsteadOfThrowing()
    {
        var result = await _handler.HandleAsync(new CreateQuizQuestionCommand(
            Guid.NewGuid(), null, (int)QuizType.CourseExam, "Pregunta", 60m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _coursesMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), default), Times.Never);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }
}
