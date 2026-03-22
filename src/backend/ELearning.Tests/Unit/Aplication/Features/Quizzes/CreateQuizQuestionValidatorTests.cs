using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class CreateQuizQuestionValidatorTests
{
    private readonly CreateQuizQuestionValidator _validator = new();

    private static CreateQuizQuestionCommand Valid() =>
        new(
            LessonId: Guid.NewGuid(),
            CourseId: null,
            Type: 0,
            QuestionText: "¿Qué es C#?",
            PassScore: 70,
            MaxAttempts: 3,
            IsRequired: true);

    [Fact]
    public void Validate_WithoutLessonIdAndCourseId_HasFkError()
    {
        var result = _validator.Validate(Valid() with { LessonId = null, CourseId = null });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FK");
    }

    [Fact]
    public void Validate_WithLessonIdAndCourseId_HasFkError()
    {
        var result = _validator.Validate(Valid() with { CourseId = Guid.NewGuid() });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FK");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public void Validate_InvalidType_HasTypeError(int type)
    {
        var result = _validator.Validate(Valid() with { Type = type });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizQuestionCommand.Type));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyQuestionText_HasQuestionTextError(string? text)
    {
        var result = _validator.Validate(Valid() with { QuestionText = text! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizQuestionCommand.QuestionText));
    }

    [Fact]
    public void Validate_QuestionTextTooLong_HasQuestionTextError()
    {
        var result = _validator.Validate(Valid() with { QuestionText = new string('A', 501) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizQuestionCommand.QuestionText));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(100.1)]
    public void Validate_PassScoreOutOfRange_HasPassScoreError(decimal passScore)
    {
        var result = _validator.Validate(Valid() with { PassScore = passScore });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizQuestionCommand.PassScore));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_MaxAttemptsNotPositive_HasMaxAttemptsError(int maxAttempts)
    {
        var result = _validator.Validate(Valid() with { MaxAttempts = maxAttempts });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizQuestionCommand.MaxAttempts));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
