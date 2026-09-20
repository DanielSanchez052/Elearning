using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class UpdateQuizQuestionHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly UpdateQuizQuestionHandler _handler;

    public UpdateQuizQuestionHandlerTests() =>
        _handler = new UpdateQuizQuestionHandler(_quizzesMock.Object);

    [Fact]
    public async Task HandleAsync_EmptyQuestionId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new UpdateQuizQuestionCommand(
            Guid.Empty, "Texto", 60m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _quizzesMock.Verify(r => r.GetQuestionByIdAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmptyQuestionText_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new UpdateQuizQuestionCommand(
            Guid.NewGuid(), "   ", 60m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task HandleAsync_PassScoreOutOfRange_ReturnsValidationFailure(decimal passScore)
    {
        var result = await _handler.HandleAsync(new UpdateQuizQuestionCommand(
            Guid.NewGuid(), "Texto", passScore, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_MaxAttemptsZero_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new UpdateQuizQuestionCommand(
            Guid.NewGuid(), "Texto", 60m, 0, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_QuestionNotFound_ReturnsNotFound()
    {
        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((QuizQuestion?)null);

        var result = await _handler.HandleAsync(new UpdateQuizQuestionCommand(
            Guid.NewGuid(), "Texto", 60m, 3, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _quizzesMock.Verify(r => r.UpdateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidUpdate_UpdatesAndSavesQuestion()
    {
        var question = QuizQuestion.CreatePerLesson(Guid.NewGuid(), "Original", 60m, 3, 1, true);

        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(question.Id, default))
            .ReturnsAsync(question);
        _quizzesMock
            .Setup(r => r.UpdateQuestionAsync(question, default))
            .Returns(Task.CompletedTask);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new UpdateQuizQuestionCommand(
            question.Id, "Actualizado", 80m, 5, false));

        Assert.True(result.IsSuccess);
        Assert.Equal("Actualizado", question.QuestionText);
        Assert.Equal(80m, question.PassScore);
        Assert.Equal(5, question.MaxAttempts);
        Assert.False(question.IsRequired);
        _quizzesMock.Verify(r => r.UpdateQuestionAsync(question, default), Times.Once);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
