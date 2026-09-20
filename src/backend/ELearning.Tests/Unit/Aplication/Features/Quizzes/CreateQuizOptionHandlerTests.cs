using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class CreateQuizOptionHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly CreateQuizOptionHandler _handler;

    public CreateQuizOptionHandlerTests() =>
        _handler = new CreateQuizOptionHandler(_quizzesMock.Object);

    [Fact]
    public async Task HandleAsync_EmptyQuestionId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new CreateQuizOptionCommand(
            Guid.Empty, "Opcion", true, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _quizzesMock.Verify(r => r.GetQuestionByIdAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmptyOptionText_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new CreateQuizOptionCommand(
            Guid.NewGuid(), "  ", true, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_OrderIndexZeroOrLess_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new CreateQuizOptionCommand(
            Guid.NewGuid(), "Opcion", true, 0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_QuestionNotFound_ReturnsNotFound()
    {
        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((QuizQuestion?)null);

        var result = await _handler.HandleAsync(new CreateQuizOptionCommand(
            Guid.NewGuid(), "Opcion", true, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _quizzesMock.Verify(r => r.CreateOptionAsync(It.IsAny<QuizOption>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidOption_CreatesAndSavesOption()
    {
        var question = QuizQuestion.CreatePerLesson(Guid.NewGuid(), "Pregunta", 60m, 3, 1, true);
        QuizOption? created = null;

        _quizzesMock
            .Setup(r => r.GetQuestionByIdAsync(question.Id, default))
            .ReturnsAsync(question);
        _quizzesMock
            .Setup(r => r.CreateOptionAsync(It.IsAny<QuizOption>(), default))
            .Callback<QuizOption, CancellationToken>((o, _) => created = o)
            .ReturnsAsync((QuizOption o, CancellationToken _) => o);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new CreateQuizOptionCommand(
            question.Id, "Opcion correcta", true, 2));

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal(question.Id, created!.QuestionId);
        Assert.Equal("Opcion correcta", created.OptionText);
        Assert.True(created.IsCorrect);
        Assert.Equal(2, created.OrderIndex);
        Assert.Equal(result.Value, created.Id);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
