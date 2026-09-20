using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class GetCourseExamQuestionsAdminHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly GetCourseExamQuestionsAdminHandler _handler;

    public GetCourseExamQuestionsAdminHandlerTests() =>
        _handler = new GetCourseExamQuestionsAdminHandler(_quizzesMock.Object);

    [Fact]
    public async Task HandleAsync_EmptyCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetCourseExamQuestionsAdminQuery(Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_NoQuestions_ReturnsEmptyListNotError()
    {
        var courseId = Guid.NewGuid();

        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(courseId, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        var result = await _handler.HandleAsync(new GetCourseExamQuestionsAdminQuery(courseId));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsMappedDtoWithOptionsInOrderIncludingIsCorrect()
    {
        var courseId = Guid.NewGuid();

        var question = QuizQuestion.CreateCourseExam(courseId, "Pregunta final", passScore: 70m, maxAttempts: 2, orderIndex: 1, isRequired: false);
        var optionB = QuizOption.Create(question.Id, "Incorrecta", isCorrect: false, orderIndex: 2);
        var optionA = QuizOption.Create(question.Id, "Correcta", isCorrect: true, orderIndex: 1);
        question.AddOption(optionB);
        question.AddOption(optionA);

        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(courseId, default))
            .ReturnsAsync([question]);

        var result = await _handler.HandleAsync(new GetCourseExamQuestionsAdminQuery(courseId));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var dto = result.Value[0];

        Assert.Equal(question.Id, dto.Id);
        Assert.Equal("Pregunta final", dto.QuestionText);
        Assert.Equal((int)QuizType.CourseExam, dto.Type);
        Assert.False(dto.IsRequired);
        Assert.Equal(70m, dto.PassScore);
        Assert.Equal(2, dto.MaxAttempts);
        Assert.Equal(1, dto.OrderIndex);
        Assert.Null(dto.LessonId);
        Assert.Equal(courseId, dto.CourseId);

        Assert.Equal(2, dto.Options.Count);
        Assert.Equal(optionA.Id, dto.Options[0].Id);
        Assert.Equal("Correcta", dto.Options[0].OptionText);
        Assert.True(dto.Options[0].IsCorrect);
        Assert.Equal(optionB.Id, dto.Options[1].Id);
        Assert.Equal("Incorrecta", dto.Options[1].OptionText);
        Assert.False(dto.Options[1].IsCorrect);
    }
}
