using ELearning.Application.Features.Lessons.Commands;
using ELearning.Application.Features.Lessons.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Lessons;

public class CreateLessonValidatorTests
{
    private readonly CreateLessonValidator _validator = new();

    private static CreateLessonCommand Valid() =>
        new(
            CourseId: Guid.NewGuid(),
            Title: "Introducción",
            Type: "video",
            ContentUrl: "https://cdn.test/video.mp4",
            IsRequired: true,
            RequesterId: Guid.NewGuid(),
            RequesterRole: "instructor");

    [Fact]
    public void Validate_EmptyCourseId_HasCourseIdError()
    {
        var result = _validator.Validate(Valid() with { CourseId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLessonCommand.CourseId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_HasTitleError(string? title)
    {
        var result = _validator.Validate(Valid() with { Title = title! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLessonCommand.Title));
    }

    [Fact]
    public void Validate_TitleTooLong_HasTitleError()
    {
        var result = _validator.Validate(Valid() with { Title = new string('A', 201) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLessonCommand.Title));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyType_HasTypeError(string? type)
    {
        var result = _validator.Validate(Valid() with { Type = type! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLessonCommand.Type));
    }

    [Fact]
    public void Validate_InvalidType_HasTypeError()
    {
        var result = _validator.Validate(Valid() with { Type = "audio" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLessonCommand.Type));
    }

    [Fact]
    public void Validate_ValidTypeAnyCase_IsValid()
    {
        var result = _validator.Validate(Valid() with { Type = "VIDEO" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("video")]
    [InlineData("pdf")]
    public void Validate_TypeRequiresContentUrl_WithoutContentUrl_HasContentUrlError(string type)
    {
        var result = _validator.Validate(Valid() with { Type = type, ContentUrl = "   " });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLessonCommand.ContentUrl));
    }

    [Fact]
    public void Validate_QuizWithoutContentUrl_IsValid()
    {
        var result = _validator.Validate(Valid() with { Type = "quiz", ContentUrl = null });

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(CreateLessonCommand.ContentUrl));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
