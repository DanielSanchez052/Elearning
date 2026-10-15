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

    private static Course BuildCourse(Guid ownerId, bool isGlobal = false, params int[] countryIds)
    {
        var course = Course.Create("Título", "Desc", null, ownerId, isGlobal);
        foreach (var countryId in countryIds)
            course.CourseCountries.Add(CourseCountry.Create(course.Id, countryId));
        return course;
    }

    private void SetupCourse(Course course) =>
        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);

    private void SetupActiveCountry(int id, string code, string name) =>
        _countriesMock
            .Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync(Country.Create(code, name));

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var cmd = new AssignCourseCountriesCommand(Guid.NewGuid(), [1], Guid.NewGuid(), "admin", 57);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorNotOwner_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid(), countryIds: 57);
        SetupCourse(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [57], Guid.NewGuid(), "instructor", 57);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_GlobalCourse_SuperAdmin_ReturnsConflict()
    {
        var course = BuildCourse(Guid.NewGuid(), isGlobal: true);
        SetupCourse(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [1], Guid.NewGuid(), "superadmin", 1);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Theory]
    [InlineData("instructor")]
    [InlineData("admin")]
    public async Task HandleAsync_GlobalCourse_NonSuperAdmin_ReturnsForbidden(string role)
    {
        var ownerId = Guid.NewGuid();
        var course = BuildCourse(ownerId, isGlobal: true);
        SetupCourse(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [57], ownerId, role, 57);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_CountryNotFound_ReturnsNotFound()
    {
        var course = BuildCourse(Guid.NewGuid());
        SetupCourse(course);
        _countriesMock
            .Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Country?)null);

        var cmd = new AssignCourseCountriesCommand(course.Id, [99], Guid.NewGuid(), "superadmin", 1);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InactiveCountry_ReturnsConflict()
    {
        var course = BuildCourse(Guid.NewGuid(), countryIds: 52);
        var country = Country.Create("CO", "Colombia");
        country.Deactivate();

        SetupCourse(course);
        SetupActiveCountry(52, "MX", "México");
        _countriesMock
            .Setup(r => r.GetByIdAsync(57, default))
            .ReturnsAsync(country);

        var cmd = new AssignCourseCountriesCommand(course.Id, [52, 57], Guid.NewGuid(), "superadmin", 1);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_DeduplicatesCountryIdsAndCallsSetCourseCountriesAsync()
    {
        var course = BuildCourse(Guid.NewGuid());
        SetupCourse(course);
        SetupActiveCountry(57, "CO", "Colombia");
        SetupActiveCountry(52, "MX", "México");

        var cmd = new AssignCourseCountriesCommand(course.Id, [57, 52, 57], Guid.NewGuid(), "superadmin", 1);
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        _coursesMock.Verify(r => r.SetCourseCountriesAsync(course.Id, It.Is<List<int>>(ids => ids.Count == 2 && ids.Contains(57) && ids.Contains(52)), default), Times.Once);
    }

    // ── Alcance por país ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_AdminRemovesOwnCountryFromSharedCourse_Succeeds()
    {
        var course = BuildCourse(Guid.NewGuid(), countryIds: [52, 57]);
        SetupCourse(course);
        SetupActiveCountry(52, "MX", "México");

        var cmd = new AssignCourseCountriesCommand(course.Id, [52], Guid.NewGuid(), "admin", 57);
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        _coursesMock.Verify(r => r.SetCourseCountriesAsync(course.Id, It.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 52), default), Times.Once);
    }

    [Theory]
    [InlineData(new[] { 57, 52 })]  // añade otro país
    [InlineData(new[] { 52 })]      // cambia su país por otro
    public async Task HandleAsync_AdminTouchesAnotherCountry_ReturnsForbidden(int[] newCountryIds)
    {
        var course = BuildCourse(Guid.NewGuid(), countryIds: 57);
        SetupCourse(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [.. newCountryIds], Guid.NewGuid(), "admin", 57);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        _coursesMock.Verify(r => r.SetCourseCountriesAsync(It.IsAny<Guid>(), It.IsAny<List<int>>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AdminOfCountryNotInCourse_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid(), countryIds: 52);
        SetupCourse(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [52, 57], Guid.NewGuid(), "admin", 57);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InstructorOwnerAddsAnotherCountry_ReturnsForbidden()
    {
        var course = BuildCourse(Guid.NewGuid(), countryIds: 57);
        SetupCourse(course);

        var cmd = new AssignCourseCountriesCommand(course.Id, [57, 52], course.CreatedBy, "instructor", 57);
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }
}
