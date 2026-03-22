using ELearning.Application.Features.Courses.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class CreateCourseHandlerTests
{
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly CreateCourseHandler _handler;

    public CreateCourseHandlerTests() =>
        _handler = new CreateCourseHandler(_coursesMock.Object);

    private static CreateCourseCommand Valid(bool isGlobal = false) =>
        new(
            Title: "  Curso de C#  ",
            Description: "  Descripción test  ",
            ThumbnailUrl: "https://cdn.test/course.png",
            IsGlobal: isGlobal,
            CreatedBy: Guid.NewGuid(),
            CreatorCountryId: 57);

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithCourseId()
    {
        var result = await _handler.HandleAsync(Valid());

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task HandleAsync_NonGlobalCourse_AssignsCreatorCountry()
    {
        Guid createdCourseId = Guid.Empty;

        _coursesMock
            .Setup(r => r.CreateAsync(It.IsAny<Course>(), default))
            .Callback<Course, CancellationToken>((c, _) => createdCourseId = c.Id)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(Valid(isGlobal: false));

        _coursesMock.Verify(r => r.SetCourseCountriesAsync(createdCourseId, It.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 57), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GlobalCourse_DoesNotAssignCountries()
    {
        await _handler.HandleAsync(Valid(isGlobal: true));

        _coursesMock.Verify(r => r.SetCourseCountriesAsync(It.IsAny<Guid>(), It.IsAny<List<int>>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_StoresTrimmedTitleAndDescription()
    {
        Course? created = null;

        _coursesMock
            .Setup(r => r.CreateAsync(It.IsAny<Course>(), default))
            .Callback<Course, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(Valid());

        Assert.NotNull(created);
        Assert.Equal("Curso de C#", created!.Title);
        Assert.Equal("Descripción test", created.Description);
    }
}
