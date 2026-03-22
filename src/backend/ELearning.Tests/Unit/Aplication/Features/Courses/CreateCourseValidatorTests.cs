using ELearning.Application.Features.Courses.Commands;
using ELearning.Application.Features.Courses.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class CreateCourseValidatorTests
{
    private readonly CreateCourseValidator _validator = new();

    private static CreateCourseCommand Valid() =>
        new(
            Title: "Curso de C#",
            Description: "Descripción",
            ThumbnailUrl: "https://cdn.test/thumb.png",
            IsGlobal: false,
            CreatedBy: Guid.NewGuid(),
            CreatorCountryId: 1);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_HasTitleError(string? title)
    {
        var result = _validator.Validate(Valid() with { Title = title! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCourseCommand.Title));
    }

    [Fact]
    public void Validate_TitleTooLong_HasTitleError()
    {
        var result = _validator.Validate(Valid() with { Title = new string('A', 201) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCourseCommand.Title));
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasDescriptionError()
    {
        var result = _validator.Validate(Valid() with { Description = new string('A', 2001) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCourseCommand.Description));
    }

    [Fact]
    public void Validate_EmptyCreatedBy_HasCreatedByError()
    {
        var result = _validator.Validate(Valid() with { CreatedBy = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCourseCommand.CreatedBy));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
