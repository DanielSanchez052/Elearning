using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class AssignCourseCountriesHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly AssignCourseCountriesHandler _handler;

    public AssignCourseCountriesHandlerTests() =>
        _handler = new AssignCourseCountriesHandler(_coursesMock.Object, _countriesMock.Object);

    private static Course BuildCourse(Guid ownerId, bool isGlobal = false) =>
        Course.Create("Título", "Desc", null, ownerId, isGlobal);

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var cmd = new AssignCourseCountriesCommand(Guid.NewGuid(), [1], Guid.NewGuid(), "admin");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorNotOwner_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid());
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [1], Guid.NewGuid(), "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_GlobalCourse_ReturnsConflict()
    {
        var course = BuildCourse(Guid.NewGuid(), isGlobal: true);
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [1], course.CreatedBy, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_CountryNotFound_ReturnsNotFound()
    {
        var course = BuildCourse(Guid.NewGuid());
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _countriesMock
            .Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Country?)null);

        var cmd = new AssignCourseCountriesCommand(course.Id, [99], course.CreatedBy, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InactiveCountry_ReturnsConflict()
    {
        var course = BuildCourse(Guid.NewGuid());
        var country = Country.Create("CO", "Colombia");
        country.Deactivate();

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _countriesMock
            .Setup(r => r.GetByIdAsync(57, default))
            .ReturnsAsync(country);

        var cmd = new AssignCourseCountriesCommand(course.Id, [57], course.CreatedBy, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_DeduplicatesCountryIdsAndCallsSetCourseCountriesAsync()
    {
        var course = BuildCourse(Guid.NewGuid());
        var country1 = Country.Create("CO", "Colombia");
        var country2 = Country.Create("MX", "México");

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _countriesMock
            .Setup(r => r.GetByIdAsync(57, default))
            .ReturnsAsync(country1);
        _countriesMock
            .Setup(r => r.GetByIdAsync(52, default))
            .ReturnsAsync(country2);

        var cmd = new AssignCourseCountriesCommand(course.Id, [57, 52, 57], course.CreatedBy, "instructor");
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        _coursesMock.Verify(r => r.SetCourseCountriesAsync(course.Id, It.Is<List<int>>(ids => ids.Count == 2 && ids.Contains(57) && ids.Contains(52)), default), Times.Once);
    }
}
