using ELearning.Application.Features.Courses.Commands;
using ELearning.Application.Features.Courses.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class AssignCourseCountriesValidatorTests
{
    private readonly AssignCourseCountriesValidator _validator = new();

    private static AssignCourseCountriesCommand Valid() =>
        new(
            CourseId: Guid.NewGuid(),
            CountryIds: [1, 2],
            RequesterId: Guid.NewGuid(),
            RequesterRole: "admin");

    [Fact]
    public void Validate_EmptyCourseId_HasCourseIdError()
    {
        var result = _validator.Validate(Valid() with { CourseId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignCourseCountriesCommand.CourseId));
    }

    [Fact]
    public void Validate_NullCountryIds_HasCountryIdsError()
    {
        var result = _validator.Validate(Valid() with { CountryIds = null! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignCourseCountriesCommand.CountryIds));
    }

    [Fact]
    public void Validate_EmptyCountryIds_HasCountryIdsError()
    {
        var result = _validator.Validate(Valid() with { CountryIds = [] });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignCourseCountriesCommand.CountryIds));
    }

    [Fact]
    public void Validate_CountryIdsContainInvalidId_HasCountryIdsError()
    {
        var result = _validator.Validate(Valid() with { CountryIds = [1, 0, -3] });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignCourseCountriesCommand.CountryIds));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
