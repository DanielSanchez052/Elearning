using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class UpdateQuizQuestionValidatorTests
{
    private readonly UpdateQuizQuestionValidator _validator = new();

    private static UpdateQuizQuestionCommand Valid() =>
        new(
            QuestionId: Guid.NewGuid(),
            QuestionText: "Pregunta actualizada",
            PassScore: 70,
            MaxAttempts: 3,
            IsRequired: true);

    [Fact]
    public void Validate_EmptyQuestionId_HasQuestionIdError()
    {
        var result = _validator.Validate(Valid() with { QuestionId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateQuizQuestionCommand.QuestionId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyQuestionText_HasQuestionTextError(string? text)
    {
        var result = _validator.Validate(Valid() with { QuestionText = text! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateQuizQuestionCommand.QuestionText));
    }

    [Fact]
    public void Validate_QuestionTextTooLong_HasQuestionTextError()
    {
        var result = _validator.Validate(Valid() with { QuestionText = new string('A', 501) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateQuizQuestionCommand.QuestionText));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(100.1)]
    public void Validate_PassScoreOutOfRange_HasPassScoreError(decimal passScore)
    {
        var result = _validator.Validate(Valid() with { PassScore = passScore });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateQuizQuestionCommand.PassScore));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_MaxAttemptsNotPositive_HasMaxAttemptsError(int maxAttempts)
    {
        var result = _validator.Validate(Valid() with { MaxAttempts = maxAttempts });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateQuizQuestionCommand.MaxAttempts));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
