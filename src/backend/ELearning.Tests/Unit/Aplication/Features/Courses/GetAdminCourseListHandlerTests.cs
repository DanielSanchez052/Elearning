using ELearning.Application.Features.Courses.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class GetAdminCourseListHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly GetAdminCourseListHandler _handler;

    public GetAdminCourseListHandlerTests() =>
        _handler = new GetAdminCourseListHandler(_coursesMock.Object, _lessonsMock.Object);

    private static Course BuildCourse(string title)
    {
        var instructor = User.Create("Grace Hopper", "grace@test.com", "hash", 1, UserRole.Instructor);
        var course = Course.Create(title, "Desc", "thumb.png", instructor.Id, isGlobal: false, instructorId: instructor.Id);
        Helpers.SetPrivate(course, nameof(Course.CreatedByUser), instructor);
        Helpers.SetPrivate(course, nameof(Course.Instructor), instructor);
        return course;
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_MapsDtosAndComputesTotals()
    {
        var c1 = BuildCourse("Curso Admin 1");

        _coursesMock
            .Setup(r => r.GetAdminListAsync(c1.InstructorId, false, 57, true, "admin", 2, 10, default))
            .ReturnsAsync(([c1], 15));
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(c1.Id, default))
            .ReturnsAsync([
                Lesson.Create(c1.Id, "L1", LessonType.Video, "v1", 1),
                Lesson.Create(c1.Id, "L2", LessonType.Quiz, null, 2)
            ]);

        var result = await _handler.HandleAsync(new GetAdminCourseListQuery(c1.InstructorId, false, 57, true, "admin", 2, 10));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(15, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items[0].LessonCount);
        Assert.Equal(c1.InstructorId, result.Value.Items[0].InstructorId);
        Assert.Equal("Grace Hopper", result.Value.Items[0].InstructorName);
    }

    [Fact]
    public async Task HandleAsync_InvalidPageAndPageSize_UsesClampedValues()
    {
        _coursesMock
            .Setup(r => r.GetAdminListAsync(null, false, null, null, null, 1, 100, default))
            .ReturnsAsync((Array.Empty<Course>(), 0));

        var result = await _handler.HandleAsync(new GetAdminCourseListQuery(null, false, null, null, null, 0, 999));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(100, result.Value.PageSize);
        _coursesMock.Verify(r => r.GetAdminListAsync(null, false, null, null, null, 1, 100, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UnassignedFilter_IsPassedToRepository_AndMapsNullInstructor()
    {
        var creator = User.Create("Admin", "admin@test.com", "hash", 1, UserRole.Admin);
        var course = Course.Create("Sin instructor", null, null, creator.Id, isGlobal: false);
        Helpers.SetPrivate(course, nameof(Course.CreatedByUser), creator);

        _coursesMock
            .Setup(r => r.GetAdminListAsync(null, true, null, null, null, 1, 20, default))
            .ReturnsAsync(([course], 1));
        _lessonsMock
            .Setup(r => r.GetByCourseAsync(course.Id, default))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetAdminCourseListQuery(null, true, null, null, null));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Items[0].InstructorId);
        Assert.Null(result.Value.Items[0].InstructorName);
    }
}
