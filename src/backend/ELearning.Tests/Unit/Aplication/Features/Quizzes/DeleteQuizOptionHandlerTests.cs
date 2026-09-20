using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class DeleteQuizOptionHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly DeleteQuizOptionHandler _handler;

    public DeleteQuizOptionHandlerTests() =>
        _handler = new DeleteQuizOptionHandler(_quizzesMock.Object);

    [Fact]
    public async Task HandleAsync_EmptyOptionId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new DeleteQuizOptionCommand(Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        _quizzesMock.Verify(r => r.GetOptionByIdAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OptionNotFound_ReturnsNotFound()
    {
        _quizzesMock
            .Setup(r => r.GetOptionByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((QuizOption?)null);

        var result = await _handler.HandleAsync(new DeleteQuizOptionCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _quizzesMock.Verify(r => r.DeleteOptionAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidOption_DeletesAndSaves()
    {
        var option = QuizOption.Create(Guid.NewGuid(), "Opcion", false, 1);

        _quizzesMock
            .Setup(r => r.GetOptionByIdAsync(option.Id, default))
            .ReturnsAsync(option);
        _quizzesMock
            .Setup(r => r.DeleteOptionAsync(option.Id, default))
            .Returns(Task.CompletedTask);
        _quizzesMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new DeleteQuizOptionCommand(option.Id));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.DeleteOptionAsync(option.Id, default), Times.Once);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
