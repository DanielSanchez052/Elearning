using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.Queries;
using ELearning.Application.Features.Enrollments.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Tests.Unit;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Enrollments;

public class GetMyEnrollmentsHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly GetMyEnrollmentsHandler _handler;

    public GetMyEnrollmentsHandlerTests() =>
        _handler = new GetMyEnrollmentsHandler(_enrollmentsMock.Object);

    [Fact]
    public async Task HandleAsync_NoEnrollments_ReturnsEmptyList()
    {
        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(Array.Empty<CourseEnrollment>());

        var result = await _handler.HandleAsync(new GetMyEnrollmentsQuery(Guid.NewGuid()));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WithEnrollments_MapsToDtoCorrectly()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        // Define fixed dates for deterministic test
        var enrolledAt = DateTime.UtcNow.AddDays(-20);
        var completedAt = DateTime.UtcNow.AddDays(-5);

        // Create course with lessons first
        var course = Course.Create("Curso de prueba", "Desc", "thumb.jpg", Guid.NewGuid(), isGlobal: false);
        var instructor = User.Create("Profesor Test", "prof@test.com", "hash", 1, UserRole.Instructor);
        Helpers.SetPrivate(course, nameof(Course.CreatedByUser), instructor);
        var lesson1 = Lesson.Create(courseId, "Lección 1", LessonType.Video, "v1.mp4", 1, true);
        var lesson2 = Lesson.Create(courseId, "Lección 2", LessonType.Pdf, "p1.pdf", 2, true);
        course.Lessons.Add(lesson1);
        course.Lessons.Add(lesson2);

        // Create enrollment and set its Course property to the course we just created
        var enrollment = CourseEnrollment.Create(userId, courseId, null); // deadlineAt null
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Id), Guid.NewGuid()); // though Create sets it, we set it again to be explicit? Actually Create sets Id to new Guid, so we don't need to set it.
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.EnrolledAt), enrolledAt);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.CompletedAt), completedAt);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Course), course);
        // Set status to Completed for the test
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Status), EnrollmentStatus.Completed);

        var completedProgress = UserLessonProgress.Create(enrollment.Id, lesson1.Id);
        completedProgress.MarkComplete();
        enrollment.LessonProgress.Add(completedProgress);

        _enrollmentsMock
            .Setup(r => r.GetByUserAsync(userId, default))
            .ReturnsAsync(new List<CourseEnrollment> { enrollment });

        var result = await _handler.HandleAsync(new GetMyEnrollmentsQuery(userId));

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value);
        Assert.Equal(enrollment.Id, dto.EnrollmentId);
        Assert.Equal(courseId, dto.CourseId);
        Assert.Equal(course.Title, dto.CourseTitle);
        Assert.Equal(course.ThumbnailUrl, dto.CourseThumbnailUrl);
        Assert.Equal(EnrollmentStatus.Completed, dto.Status);
        Assert.Equal(2, dto.TotalLessons);
        Assert.Equal(2, dto.RequiredLessons);
        Assert.Equal(1, dto.CompletedLessons);
        Assert.Equal(50, dto.ProgressPercent); // 1 out of 2 required
        Assert.Equal(enrolledAt, dto.EnrolledAt);
        Assert.Equal(completedAt, dto.CompletedAt);
        Assert.Null(dto.DeadlineAt);
    }
}