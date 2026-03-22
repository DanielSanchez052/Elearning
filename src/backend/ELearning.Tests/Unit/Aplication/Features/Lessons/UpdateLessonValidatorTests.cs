using ELearning.Application.Features.Lessons.Commands;
using ELearning.Application.Features.Lessons.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Lessons;

public class UpdateLessonValidatorTests
{
    private readonly UpdateLessonValidator _validator = new();

    private static UpdateLessonCommand Valid() =>
        new(
            LessonId: Guid.NewGuid(),
            Title: "Lección actualizada",
            ContentUrl: "https://cdn.test/new.mp4",
            IsRequired: true,
            RequesterId: Guid.NewGuid(),
            RequesterRole: "instructor");

    [Fact]
    public void Validate_EmptyLessonId_HasLessonIdError()
    {
        var result = _validator.Validate(Valid() with { LessonId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateLessonCommand.LessonId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_HasTitleError(string? title)
    {
        var result = _validator.Validate(Valid() with { Title = title! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateLessonCommand.Title));
    }

    [Fact]
    public void Validate_TitleTooLong_HasTitleError()
    {
        var result = _validator.Validate(Valid() with { Title = new string('A', 201) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateLessonCommand.Title));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
