using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class DeleteQuizQuestionHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly DeleteQuizQuestionHandler _handler;

    public DeleteQuizQuestionHandlerTests() =>
        _handler = new DeleteQuizQuestionHandler(_quizzesMock.Object);

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
        var question = QuizQuestion.CreatePerLesson(Guid.NewGuid(), "Pregunta", 60m, 3, 1, true);

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
        _quizzesMock.Verify(r => r.DeleteQuestionAsync(question.Id, default), Times.Once);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
