using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.Queries;
using ELearning.Application.Features.Enrollments.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Tests.Unit;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Enrollments;

public class GetCourseProgressHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly GetCourseProgressHandler _handler;

    public GetCourseProgressHandlerTests() =>
        _handler = new GetCourseProgressHandler(_enrollmentsMock.Object);

    [Fact]
    public async Task HandleAsync_NoEnrollment_ReturnsNotFound()
    {
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(new GetCourseProgressQuery(Guid.NewGuid(), Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_WithEnrollment_MapsToDtoCorrectly()
    {
        var userId = Guid.NewGuid();

        var course = Course.Create("Curso de prueba", "Desc", "thumb.jpg", Guid.NewGuid(), isGlobal: false);
        var instructor = User.Create("Profesor Test", "prof@test.com", "hash", 1, UserRole.Instructor);
        Helpers.SetPrivate(course, nameof(Course.CreatedByUser), instructor);

        var courseId = course.Id;

        var enrollment = CourseEnrollment.Create(userId, courseId, null); // deadlineAt null
        // Set navigation properties and dates
        var enrolledAt = DateTime.UtcNow.AddDays(-20);
        var completedAt = DateTime.UtcNow.AddDays(-5);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.EnrolledAt), enrolledAt);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.CompletedAt), completedAt);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Course), course);
        // Note: enrollment.Status is Active by default

        var lesson1 = Lesson.Create(courseId, "Lección 1", LessonType.Video, "v1.mp4", 1, true);
        var lesson2 = Lesson.Create(courseId, "Lección 2", LessonType.Pdf, "p1.pdf", 2, false); // not required
        var lesson3 = Lesson.Create(courseId, "Lección 3", LessonType.Quiz, null, 3, true); // required
        course.Lessons.Add(lesson1);
        course.Lessons.Add(lesson2);
        course.Lessons.Add(lesson3);

        var progress1 = UserLessonProgress.Create(enrollment.Id, lesson1.Id);
        progress1.MarkComplete(); // completed
        var progress2 = UserLessonProgress.Create(enrollment.Id, lesson2.Id);
        // progress2 not completed
        var progress3 = UserLessonProgress.Create(enrollment.Id, lesson3.Id);
        progress3.MarkComplete(); // completed
        enrollment.LessonProgress.Add(progress1);
        enrollment.LessonProgress.Add(progress2);
        enrollment.LessonProgress.Add(progress3);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, courseId, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new GetCourseProgressQuery(userId, courseId));

        Assert.True(result.IsSuccess);
        var dto = Assert.IsType<CourseProgressDto>(result.Value);
        Assert.NotEqual(Guid.Empty, dto.EnrollmentId);
        Assert.Equal(courseId, dto.CourseId);
        Assert.Equal(course.Title, dto.CourseTitle);
        Assert.Equal(course.ThumbnailUrl, dto.CourseThumbnailUrl);
        Assert.Equal(enrollment.Status, dto.Status);
        Assert.Equal(enrolledAt, dto.EnrolledAt);
        Assert.Equal(completedAt, dto.CompletedAt);
        Assert.Equal(2, dto.CompletedLessons); // lesson1 and lesson3 completed
        Assert.Equal(2, dto.RequiredLessons); // lesson1 and lesson3 are required
        Assert.Equal(100, dto.ProgressPercent);
        Assert.Equal(3, dto.Lessons.Count);
        Assert.Equal(lesson1.Id, dto.Lessons[0].LessonId);
        Assert.Equal(lesson1.Title, dto.Lessons[0].Title);
        Assert.Equal(LessonType.Video.ToString().ToLowerInvariant(), dto.Lessons[0].Type);
        Assert.Equal(1, dto.Lessons[0].OrderIndex);
        Assert.True(dto.Lessons[0].IsRequired);
        Assert.True(dto.Lessons[0].IsCompleted);
        Assert.Equal(lesson2.Id, dto.Lessons[1].LessonId);
        Assert.Equal(lesson2.Title, dto.Lessons[1].Title);
        Assert.Equal(LessonType.Pdf.ToString().ToLowerInvariant(), dto.Lessons[1].Type);
        Assert.Equal(2, dto.Lessons[1].OrderIndex);
        Assert.False(dto.Lessons[1].IsRequired);
        Assert.False(dto.Lessons[1].IsCompleted);
        Assert.Equal(lesson3.Id, dto.Lessons[2].LessonId);
        Assert.Equal(lesson3.Title, dto.Lessons[2].Title);
        Assert.Equal(LessonType.Quiz.ToString().ToLowerInvariant(), dto.Lessons[2].Type);
        Assert.Equal(3, dto.Lessons[2].OrderIndex);
        Assert.True(dto.Lessons[2].IsRequired);
        Assert.True(dto.Lessons[2].IsCompleted);
    }
}