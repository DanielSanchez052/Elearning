using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Enrollments;

public class EnrollInCourseHandlerTests
{
    private const int CountryId = 57;
    private const int OtherCountryId = 52;

    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly EnrollInCourseHandler _handler;

    public EnrollInCourseHandlerTests() =>
        _handler = new EnrollInCourseHandler(_enrollmentsMock.Object, _coursesMock.Object);

    private Course SetupCourse(bool isActive = true, bool isGlobal = false, params int[] countryIds)
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal);
        if (isActive)
            course.Activate();
        foreach (var countryId in countryIds)
            course.CourseCountries.Add(CourseCountry.Create(course.Id, countryId));

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        return course;
    }

    private void AssertNotEnrolled(Result<Guid> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _enrollmentsMock.Verify(r => r.AddAsync(It.IsAny<CourseEnrollment>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(new EnrollInCourseCommand(Guid.NewGuid(), Guid.NewGuid(), CountryId));

        AssertNotEnrolled(result);
    }

    [Fact]
    public async Task HandleAsync_InactiveCourse_ReturnsNotFound()
    {
        var course = SetupCourse(isActive: false, countryIds: CountryId);

        var result = await _handler.HandleAsync(new EnrollInCourseCommand(Guid.NewGuid(), course.Id, CountryId));

        AssertNotEnrolled(result);
    }

    [Fact]
    public async Task HandleAsync_CourseOfAnotherCountry_ReturnsNotFound()
    {
        var course = SetupCourse(countryIds: OtherCountryId);

        var result = await _handler.HandleAsync(new EnrollInCourseCommand(Guid.NewGuid(), course.Id, CountryId));

        AssertNotEnrolled(result);
    }

    [Fact]
    public async Task HandleAsync_AlreadyEnrolled_ReturnsConflict()
    {
        var course = SetupCourse(countryIds: CountryId);
        var userId = Guid.NewGuid();

        _enrollmentsMock
            .Setup(r => r.ExistsAsync(userId, course.Id, default))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new EnrollInCourseCommand(userId, course.Id, CountryId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Theory]
    [InlineData(false, new[] { CountryId })]                  // asignado a su país
    [InlineData(false, new[] { OtherCountryId, CountryId })]  // compartido con otro país
    [InlineData(true, new int[0])]                            // global
    public async Task HandleAsync_AvailableCourse_CreatesAndSavesEnrollment(bool isGlobal, int[] countryIds)
    {
        var course = SetupCourse(isGlobal: isGlobal, countryIds: countryIds);
        CourseEnrollment? createdEnrollment = null;

        _enrollmentsMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), course.Id, default))
            .ReturnsAsync(false);
        _enrollmentsMock
            .Setup(r => r.AddAsync(It.IsAny<CourseEnrollment>(), default))
            .Callback<CourseEnrollment, CancellationToken>((ce, _) => createdEnrollment = ce)
            .Returns(Task.CompletedTask);
        _enrollmentsMock
            .Setup(r => r.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var userId = Guid.NewGuid();
        var result = await _handler.HandleAsync(new EnrollInCourseCommand(userId, course.Id, CountryId));

        Assert.True(result.IsSuccess);
        Assert.NotNull(createdEnrollment);
        Assert.Equal(userId, createdEnrollment!.UserId);
        Assert.Equal(course.Id, createdEnrollment.CourseId);
        _enrollmentsMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
