using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class CreateQuizOptionValidatorTests
{
    private readonly CreateQuizOptionValidator _validator = new();

    private static CreateQuizOptionCommand Valid() =>
        new(
            QuestionId: Guid.NewGuid(),
            OptionText: "Respuesta A",
            IsCorrect: false,
            OrderIndex: 1);

    [Fact]
    public void Validate_EmptyQuestionId_HasQuestionIdError()
    {
        var result = _validator.Validate(Valid() with { QuestionId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizOptionCommand.QuestionId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyOptionText_HasOptionTextError(string? optionText)
    {
        var result = _validator.Validate(Valid() with { OptionText = optionText! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizOptionCommand.OptionText));
    }

    [Fact]
    public void Validate_OptionTextTooLong_HasOptionTextError()
    {
        var result = _validator.Validate(Valid() with { OptionText = new string('A', 201) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizOptionCommand.OptionText));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_OrderIndexNotPositive_HasOrderIndexError(int orderIndex)
    {
        var result = _validator.Validate(Valid() with { OrderIndex = orderIndex });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuizOptionCommand.OrderIndex));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
