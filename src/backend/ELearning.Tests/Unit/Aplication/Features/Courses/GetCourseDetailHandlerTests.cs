using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class GetCourseDetailHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly GetCourseDetailHandler _handler;

    public GetCourseDetailHandlerTests() =>
        _handler = new GetCourseDetailHandler(_coursesMock.Object, _lessonsMock.Object);

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(new GetCourseDetailQuery(Guid.NewGuid()));

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

        var result = await _handler.HandleAsync(new GetCourseDetailQuery(course.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("Curso Backend", result.Value.Title);
        Assert.Equal("Linus Torvalds", result.Value.InstructorName);
        Assert.Equal(2, result.Value.Lessons.Count);
        Assert.Equal("video", result.Value.Lessons[0].Type);
        Assert.Equal("quiz", result.Value.Lessons[1].Type);
        Assert.Single(result.Value.Countries);
        Assert.Equal("CO", result.Value.Countries[0].Code);
        Assert.Equal("Colombia", result.Value.Countries[0].Name);
    }
}
