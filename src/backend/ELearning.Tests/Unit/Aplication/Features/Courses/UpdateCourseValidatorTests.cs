using ELearning.Application.Features.Courses.Commands;
using ELearning.Application.Features.Courses.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class UpdateCourseValidatorTests
{
    private readonly UpdateCourseValidator _validator = new();

    private static UpdateCourseCommand Valid() =>
        new(
            CourseId: Guid.NewGuid(),
            Title: "Curso de C#",
            Description: "Descripción",
            ThumbnailUrl: "https://cdn.test/thumb.png",
            IsGlobal: false,
            RequesterId: Guid.NewGuid(),
            RequesterRole: "instructor");

    [Fact]
    public void Validate_EmptyCourseId_HasCourseIdError()
    {
        var result = _validator.Validate(Valid() with { CourseId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCourseCommand.CourseId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_HasTitleError(string? title)
    {
        var result = _validator.Validate(Valid() with { Title = title! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCourseCommand.Title));
    }

    [Fact]
    public void Validate_TitleTooLong_HasTitleError()
    {
        var result = _validator.Validate(Valid() with { Title = new string('A', 201) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCourseCommand.Title));
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasDescriptionError()
    {
        var result = _validator.Validate(Valid() with { Description = new string('A', 2001) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCourseCommand.Description));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
