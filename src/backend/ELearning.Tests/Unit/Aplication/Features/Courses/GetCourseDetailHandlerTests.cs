using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class GetCourseDetailHandlerTests
{
    private const int CountryId = 57;
    private const int OtherCountryId = 52;

    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly GetCourseDetailHandler _handler;

    public GetCourseDetailHandlerTests() =>
        _handler = new GetCourseDetailHandler(_coursesMock.Object, _lessonsMock.Object, _enrollmentsMock.Object);

    private Course SetupCourse(bool isActive = true, bool isGlobal = false, Guid? ownerId = null, params int[] countryIds)
    {
        var instructor = User.Create("Linus Torvalds", "linus@test.com", "hash", CountryId, UserRole.Instructor);
        var course = Course.Create("Curso Backend", "Desc", "thumb.png", ownerId ?? instructor.Id, isGlobal);
        Helpers.SetPrivate(course, nameof(Course.CreatedByUser), instructor);
        if (isActive)
            course.Activate();
        foreach (var countryId in countryIds)
            course.CourseCountries.Add(CourseCountry.Create(course.Id, countryId));

        _coursesMock.Setup(r => r.GetByIdAsync(course.Id, default)).ReturnsAsync(course);
        _coursesMock.Setup(r => r.GetCourseCountriesAsync(course.Id, default)).ReturnsAsync([]);
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(course.Id, default))
            .ReturnsAsync([Lesson.Create(course.Id, "Intro", LessonType.Video, "video.mp4", 1, true)]);
        return course;
    }

    private static GetCourseDetailQuery Query(Course course, string role = "student", int countryId = CountryId, Guid? requesterId = null) =>
        new(course.Id, requesterId ?? Guid.NewGuid(), role, countryId);

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(new GetCourseDetailQuery(Guid.NewGuid(), Guid.NewGuid(), "student", CountryId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ExistingCourse_ReturnsMappedDetailDto()
    {
        var instructor = User.Create("Linus Torvalds", "linus@test.com", "hash", 1, UserRole.Instructor);
        var course = Course.Create("Curso Backend", "Desc", "thumb.png", instructor.Id, isGlobal: false);
        Helpers.SetPrivate(course, nameof(Course.CreatedByUser), instructor);

        var lesson1 = Lesson.Create(course.Id, "Intro", LessonType.Video, "video.mp4", 1, true);
        var lesson2 = Lesson.Create(course.Id, "Quiz", LessonType.Quiz, null, 2, false);

        var country = Country.Create("CO", "Colombia");
        Helpers.SetPrivate(country, nameof(Country.Id), 57);
        var courseCountry = CourseCountry.Create(course.Id, 57);
        Helpers.SetPrivate(courseCountry, nameof(CourseCountry.Country), country);

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(course.Id, default))
            .ReturnsAsync([lesson1, lesson2]);
        _coursesMock
            .Setup(r => r.GetCourseCountriesAsync(course.Id, default))
            .ReturnsAsync([courseCountry]);

        var result = await _handler.HandleAsync(new GetCourseDetailQuery(course.Id, Guid.NewGuid(), "superadmin", 1));

        Assert.True(result.IsSuccess);
        Assert.Equal("Curso Backend", result.Value.Title);
        Assert.Equal("Linus Torvalds", result.Value.InstructorName);
        Assert.Equal(2, result.Value.Lessons.Count);
        Assert.Equal("video", result.Value.Lessons[0].Type);
        Assert.Equal("video.mp4", result.Value.Lessons[0].ContentUrl);
        Assert.Equal("quiz", result.Value.Lessons[1].Type);
        Assert.Single(result.Value.Countries);
        Assert.Equal("CO", result.Value.Countries[0].Code);
        Assert.Equal("Colombia", result.Value.Countries[0].Name);
    }

    // ── Visibilidad para quien no gestiona el curso ───────────────────────────

    [Fact]
    public async Task HandleAsync_StudentNotEnrolled_AvailableCourse_ReturnsDetailWithoutContentUrls()
    {
        var course = SetupCourse(countryIds: CountryId);

        var result = await _handler.HandleAsync(Query(course));

        Assert.True(result.IsSuccess);
        Assert.All(result.Value.Lessons, l => Assert.Null(l.ContentUrl));
    }

    [Fact]
    public async Task HandleAsync_StudentNotEnrolled_GlobalCourse_ReturnsDetail()
    {
        var course = SetupCourse(isGlobal: true);

        var result = await _handler.HandleAsync(Query(course, countryId: OtherCountryId));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_StudentNotEnrolled_InactiveCourse_ReturnsNotFound()
    {
        var course = SetupCourse(isActive: false, countryIds: CountryId);

        var result = await _handler.HandleAsync(Query(course));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_StudentNotEnrolled_CourseOfAnotherCountry_ReturnsNotFound()
    {
        var course = SetupCourse(countryIds: OtherCountryId);

        var result = await _handler.HandleAsync(Query(course));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Theory]
    [InlineData(false, CountryId)]       // despublicado después de inscribirse
    [InlineData(true, OtherCountryId)]   // reasignado a otro país después de inscribirse
    public async Task HandleAsync_EnrolledStudent_KeepsAccessWithContentUrls(bool isActive, int courseCountryId)
    {
        var studentId = Guid.NewGuid();
        var course = SetupCourse(isActive: isActive, countryIds: courseCountryId);
        _enrollmentsMock.Setup(r => r.ExistsAsync(studentId, course.Id, default)).ReturnsAsync(true);

        var result = await _handler.HandleAsync(Query(course, requesterId: studentId));

        Assert.True(result.IsSuccess);
        Assert.Equal("video.mp4", result.Value.Lessons[0].ContentUrl);
    }

    // ── Staff ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_InstructorOwner_InactiveCourse_ReturnsDetailWithContentUrls()
    {
        var ownerId = Guid.NewGuid();
        var course = SetupCourse(isActive: false, ownerId: ownerId, countryIds: CountryId);

        var result = await _handler.HandleAsync(Query(course, "instructor", requesterId: ownerId));

        Assert.True(result.IsSuccess);
        Assert.Equal("video.mp4", result.Value.Lessons[0].ContentUrl);
        _enrollmentsMock.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AdminOfCourseCountry_InactiveCourse_ReturnsDetail()
    {
        var course = SetupCourse(isActive: false, countryIds: CountryId);

        var result = await _handler.HandleAsync(Query(course, "admin"));

        Assert.True(result.IsSuccess);
        Assert.Equal("video.mp4", result.Value.Lessons[0].ContentUrl);
    }

    [Fact]
    public async Task HandleAsync_AdminOfAnotherCountry_InactiveCourse_ReturnsNotFound()
    {
        var course = SetupCourse(isActive: false, countryIds: OtherCountryId);

        var result = await _handler.HandleAsync(Query(course, "admin"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_AdminOnPublishedGlobalCourse_SeesItLikeAStudent()
    {
        var course = SetupCourse(isGlobal: true);

        var result = await _handler.HandleAsync(Query(course, "admin"));

        Assert.True(result.IsSuccess);
        Assert.All(result.Value.Lessons, l => Assert.Null(l.ContentUrl));
    }
}
