using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Enrollments;

public class EnrollInCourseHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly EnrollInCourseHandler _handler;

    public EnrollInCourseHandlerTests() =>
        _handler = new EnrollInCourseHandler(_enrollmentsMock.Object, _coursesMock.Object);

    [Fact]
    public async Task HandleAsync_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(new EnrollInCourseCommand(Guid.NewGuid(), Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InactiveCourse_ReturnsConflict()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        // Curso está inactivo por defecto

        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(course);

        var result = await _handler.HandleAsync(new EnrollInCourseCommand(Guid.NewGuid(), course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_AlreadyEnrolled_ReturnsConflict()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        course.Activate();
        var enrollment = CourseEnrollment.Create(Guid.NewGuid(), course.Id);

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
        _enrollmentsMock
            .Setup(r => r.ExistsAsync(enrollment.UserId, course.Id, default))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new EnrollInCourseCommand(enrollment.UserId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_ValidEnrollment_CreatesAndSavesEnrollment()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        course.Activate();
        CourseEnrollment? createdEnrollment = null;

        _coursesMock
            .Setup(r => r.GetByIdAsync(course.Id, default))
            .ReturnsAsync(course);
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
        var result = await _handler.HandleAsync(new EnrollInCourseCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.NotNull(createdEnrollment);
        Assert.Equal(userId, createdEnrollment!.UserId);
        Assert.Equal(course.Id, createdEnrollment.CourseId);
        _enrollmentsMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}