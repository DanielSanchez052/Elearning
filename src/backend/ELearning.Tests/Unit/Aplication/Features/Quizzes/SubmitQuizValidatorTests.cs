using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class SubmitQuizValidatorTests
{
    private readonly SubmitQuizValidator _validator = new();

    private static SubmitQuizCommand ValidLessonCommand() =>
        new(
            UserId: Guid.NewGuid(),
            LessonId: Guid.NewGuid(),
            CourseId: null,
            SelectedOptionIds: [Guid.NewGuid()]);

    [Fact]
    public void Validate_EmptyUserId_HasUserIdError()
    {
        var result = _validator.Validate(ValidLessonCommand() with { UserId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitQuizCommand.UserId));
    }

    [Fact]
    public void Validate_WithoutLessonIdAndCourseId_HasFkError()
    {
        var result = _validator.Validate(ValidLessonCommand() with { LessonId = null, CourseId = null });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FK");
    }

    [Fact]
    public void Validate_WithLessonIdAndCourseId_HasFkError()
    {
        var result = _validator.Validate(ValidLessonCommand() with { CourseId = Guid.NewGuid() });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FK");
    }

    [Fact]
    public void Validate_NullSelectedOptionIds_HasSelectedOptionIdsError()
    {
        var result = _validator.Validate(ValidLessonCommand() with { SelectedOptionIds = null! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitQuizCommand.SelectedOptionIds));
    }

    [Fact]
    public void Validate_EmptySelectedOptionIds_HasSelectedOptionIdsError()
    {
        var result = _validator.Validate(ValidLessonCommand() with { SelectedOptionIds = [] });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitQuizCommand.SelectedOptionIds));
    }

    [Fact]
    public void Validate_SelectedOptionIdsContainsEmptyGuid_HasSelectedOptionIdsError()
    {
        var result = _validator.Validate(ValidLessonCommand() with { SelectedOptionIds = [Guid.Empty] });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitQuizCommand.SelectedOptionIds));
    }

    [Fact]
    public void Validate_ValidLessonCommand_IsValid()
    {
        var result = _validator.Validate(ValidLessonCommand());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ValidCourseCommand_IsValid()
    {
        var cmd = new SubmitQuizCommand(
            UserId: Guid.NewGuid(),
            LessonId: null,
            CourseId: Guid.NewGuid(),
            SelectedOptionIds: [Guid.NewGuid()]);

        var result = _validator.Validate(cmd);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
