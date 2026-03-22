using ELearning.Application.Features.Courses.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class GetCourseCatalogHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly GetCourseCatalogHandler _handler;

    public GetCourseCatalogHandlerTests() =>
        _handler = new GetCourseCatalogHandler(_coursesMock.Object, _lessonsMock.Object);

    private static Course BuildCourse(string title)
    {
        var instructor = User.Create("Ada Lovelace", "ada@test.com", "hash", 1, UserRole.Instructor);
        var course = Course.Create(title, "Desc", "thumb.png", instructor.Id, isGlobal: false);
        Helpers.SetPrivate(course, nameof(Course.CreatedByUser), instructor);
        return course;
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_MapsDtosAndComputesTotals()
    {
        var c1 = BuildCourse("Curso 1");
        var c2 = BuildCourse("Curso 2");

        _coursesMock
            .Setup(r => r.GetCatalogAsync(57, "c#", 1, 20, default))
            .ReturnsAsync(([c1, c2], 7));
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(c1.Id, default))
            .ReturnsAsync([
                Lesson.Create(c1.Id, "L1", LessonType.Video, "v1", 1),
                Lesson.Create(c1.Id, "L2", LessonType.Pdf, "p1", 2)
            ]);
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(c2.Id, default))
            .ReturnsAsync([
                Lesson.Create(c2.Id, "L1", LessonType.Video, "v1", 1)
            ]);

        var result = await _handler.HandleAsync(new GetCourseCatalogQuery(57, "c#", 1, 20));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(7, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(1, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items[0].LessonCount);
        Assert.Equal(1, result.Value.Items[1].LessonCount);
    }

    [Fact]
    public async Task HandleAsync_InvalidPageAndPageSize_UsesClampedValues()
    {
        _coursesMock
            .Setup(r => r.GetCatalogAsync(57, null, 1, 50, default))
            .ReturnsAsync((Array.Empty<Course>(), 0));

        var result = await _handler.HandleAsync(new GetCourseCatalogQuery(57, null, 0, 999));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(50, result.Value.PageSize);
        _coursesMock.Verify(r => r.GetCatalogAsync(57, null, 1, 50, default), Times.Once);
    }
}
