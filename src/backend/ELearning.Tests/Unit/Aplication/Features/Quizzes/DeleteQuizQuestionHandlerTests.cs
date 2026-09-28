using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class DeleteQuizQuestionHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly DeleteQuizQuestionHandler _handler;

    public DeleteQuizQuestionHandlerTests() =>
        _handler = new DeleteQuizQuestionHandler(_quizzesMock.Object, _lessonsMock.Object);

    [Fact]
    public async Task HandleAsync_EmptyQuestionId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new DeleteQuizQuestionCommand(Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _quizzesMock.Verify(r => r.GetQuestionByIdAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_QuestionNotFound_ReturnsNotFound()
    {
        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((QuizQuestion?)null);

        var result = await _handler.HandleAsync(new DeleteQuizQuestionCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _quizzesMock.Verify(r => r.DeleteQuestionAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidQuestion_DeletesAndSaves()
    {
        var lessonId = Guid.NewGuid();
        var question = QuizQuestion.CreatePerLesson(lessonId, "Pregunta", 60m, 3, 1, true);
        var otherQuestion = QuizQuestion.CreatePerLesson(lessonId, "Otra pregunta", 60m, 3, 2, true);

        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(question.Id, default))
            .ReturnsAsync(question);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lessonId, default))
            .ReturnsAsync(new List<QuizQuestion> { question, otherQuestion });
        _quizzesMock
            .Setup(r => r.DeleteQuestionAsync(question.Id, default))
            .Returns(Task.CompletedTask);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new DeleteQuizQuestionCommand(question.Id));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.DeleteQuestionAsync(question.Id, default), Times.Once);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_LastQuestionInRequiredLesson_ReturnsValidationFailureAndDoesNotDelete()
    {
        var lesson = Lesson.Create(Guid.NewGuid(), "Quiz obligatorio", LessonType.Quiz, null, 1, isRequired: true);
        var question = QuizQuestion.CreatePerLesson(lesson.Id, "Única pregunta", 60m, 3, 1, true);

        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(question.Id, default))
            .ReturnsAsync(question);
        _lessonsMock
            .Setup(r => r.GetByIdAsync(lesson.Id, default))
            .ReturnsAsync(lesson);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync(new List<QuizQuestion> { question });

        var result = await _handler.HandleAsync(new DeleteQuizQuestionCommand(question.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _quizzesMock.Verify(r => r.DeleteQuestionAsync(It.IsAny<Guid>(), default), Times.Never);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_LastQuestionInNonRequiredLesson_DeletesSuccessfully()
    {
        var lesson = Lesson.Create(Guid.NewGuid(), "Quiz opcional", LessonType.Quiz, null, 1, isRequired: false);
        var question = QuizQuestion.CreatePerLesson(lesson.Id, "Única pregunta", 60m, 3, 1, true);

        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(question.Id, default))
            .ReturnsAsync(question);
        _lessonsMock
            .Setup(r => r.GetByIdAsync(lesson.Id, default))
            .ReturnsAsync(lesson);
        _quizzesMock
            .Setup(r => r.DeleteQuestionAsync(question.Id, default))
            .Returns(Task.CompletedTask);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new DeleteQuizQuestionCommand(question.Id));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.GetQuestionsByLessonAsync(It.IsAny<Guid>(), default), Times.Never);
        _quizzesMock.Verify(r => r.DeleteQuestionAsync(question.Id, default), Times.Once);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_LastQuestionInCourseExam_DeletesSuccessfully()
    {
        var courseId = Guid.NewGuid();
        var question = QuizQuestion.CreateCourseExam(courseId, "Única pregunta de examen", 70m, 3, 1, true);

        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(question.Id, default))
            .ReturnsAsync(question);
        _quizzesMock
            .Setup(r => r.DeleteQuestionAsync(question.Id, default))
            .Returns(Task.CompletedTask);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new DeleteQuizQuestionCommand(question.Id));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.GetQuestionsByCourseAsync(It.IsAny<Guid>(), default), Times.Never);
        _quizzesMock.Verify(r => r.DeleteQuestionAsync(question.Id, default), Times.Once);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
