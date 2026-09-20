using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class GetLessonQuizQuestionsAdminHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly GetLessonQuizQuestionsAdminHandler _handler;

    public GetLessonQuizQuestionsAdminHandlerTests() =>
        _handler = new GetLessonQuizQuestionsAdminHandler(_quizzesMock.Object);

    [Fact]
    public async Task HandleAsync_EmptyLessonId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetLessonQuizQuestionsAdminQuery(Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_NoQuestions_ReturnsEmptyListNotError()
    {
        var lessonId = Guid.NewGuid();

        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lessonId, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        var result = await _handler.HandleAsync(new GetLessonQuizQuestionsAdminQuery(lessonId));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsMappedDtoWithOptionsInOrderIncludingIsCorrect()
    {
        var lessonId = Guid.NewGuid();

        var question = QuizQuestion.CreatePerLesson(lessonId, "Pregunta de leccion", passScore: 80m, maxAttempts: 3, orderIndex: 1, isRequired: true);
        var optionB = QuizOption.Create(question.Id, "Incorrecta", isCorrect: false, orderIndex: 2);
        var optionA = QuizOption.Create(question.Id, "Correcta", isCorrect: true, orderIndex: 1);
        question.AddOption(optionB);
        question.AddOption(optionA);

        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lessonId, default))
            .ReturnsAsync([question]);

        var result = await _handler.HandleAsync(new GetLessonQuizQuestionsAdminQuery(lessonId));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var dto = result.Value[0];

        Assert.Equal(question.Id, dto.Id);
        Assert.Equal("Pregunta de leccion", dto.QuestionText);
        Assert.Equal((int)QuizType.PerLesson, dto.Type);
        Assert.True(dto.IsRequired);
        Assert.Equal(80m, dto.PassScore);
        Assert.Equal(3, dto.MaxAttempts);
        Assert.Equal(1, dto.OrderIndex);
        Assert.Equal(lessonId, dto.LessonId);
        Assert.Null(dto.CourseId);

        Assert.Equal(2, dto.Options.Count);
        Assert.Equal(optionA.Id, dto.Options[0].Id);
        Assert.Equal("Correcta", dto.Options[0].OptionText);
        Assert.True(dto.Options[0].IsCorrect);
        Assert.Equal(optionB.Id, dto.Options[1].Id);
        Assert.Equal("Incorrecta", dto.Options[1].OptionText);
        Assert.False(dto.Options[1].IsCorrect);
    }
}
