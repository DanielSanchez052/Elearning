using ELearning.Application.Features.Enrollments.Commands;
using ELearning.Application.Features.Enrollments.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Enrollments;

public class MarkLessonCompleteValidatorTests
{
    private readonly MarkLessonCompleteValidator _validator = new();

    private static MarkLessonCompleteCommand Valid() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Validate_EmptyUserId_HasUserIdError()
    {
        var result = _validator.Validate(Valid() with { UserId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MarkLessonCompleteCommand.UserId));
    }

    [Fact]
    public void Validate_EmptyCourseId_HasCourseIdError()
    {
        var result = _validator.Validate(Valid() with { CourseId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MarkLessonCompleteCommand.CourseId));
    }

    [Fact]
    public void Validate_EmptyLessonId_HasLessonIdError()
    {
        var result = _validator.Validate(Valid() with { LessonId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MarkLessonCompleteCommand.LessonId));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
