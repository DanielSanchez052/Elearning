using ELearning.Application.Features.Enrollments.Commands;
using ELearning.Application.Features.Enrollments.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Enrollments;

public class EnrollInCourseValidatorTests
{
    private readonly EnrollInCourseValidator _validator = new();

    [Fact]
    public void Validate_EmptyUserId_HasUserIdError()
    {
        var result = _validator.Validate(new EnrollInCourseCommand(Guid.Empty, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollInCourseCommand.UserId));
    }

    [Fact]
    public void Validate_EmptyCourseId_HasCourseIdError()
    {
        var result = _validator.Validate(new EnrollInCourseCommand(Guid.NewGuid(), Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollInCourseCommand.CourseId));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new EnrollInCourseCommand(Guid.NewGuid(), Guid.NewGuid()));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
