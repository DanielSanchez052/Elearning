using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.Commands;
using ELearning.Application.Features.Enrollments.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Tests.Unit;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Enrollments;

public class MarkLessonCompleteHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly MarkLessonCompleteHandler _handler;

    public MarkLessonCompleteHandlerTests() =>
        _handler = new MarkLessonCompleteHandler(_enrollmentsMock.Object, _quizzesMock.Object);

    private static (CourseEnrollment enrollment, Lesson lesson) CreateEnrollmentWithLesson(
        Guid userId,
        Guid courseId,
        bool lessonIsRequired,
        bool lessonAlreadyCompleted)
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        course.Activate();
        var lesson = Lesson.Create(courseId, "Lección", LessonType.Video, "video.mp4", 1, lessonIsRequired);
        course.Lessons.Add(lesson);

        var enrollment = CourseEnrollment.Create(userId, courseId);
        // Set navigation properties
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Course), course);
        var progress = UserLessonProgress.Create(enrollment.Id, lesson.Id);
        if (lessonAlreadyCompleted)
            progress.MarkComplete();
        enrollment.LessonProgress.Add(progress);

        return (enrollment, lesson);
    }

    [Fact]
    public async Task HandleAsync_UserNotEnrolled_ReturnsNotFound()
    {
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(new MarkLessonCompleteCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InactiveEnrollment_ReturnsConflict()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        course.Activate();
        var enrollment = CourseEnrollment.Create(Guid.NewGuid(), course.Id);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Course), course);
        enrollment.Abandon(); // make inactive

        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "video.mp4", 1);
        course.Lessons.Add(lesson);
        var progress = UserLessonProgress.Create(enrollment.Id, lesson.Id);
        enrollment.LessonProgress.Add(progress);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(enrollment.UserId, enrollment.CourseId, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new MarkLessonCompleteCommand(enrollment.UserId, enrollment.CourseId, lesson.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_LessonNotInCourse_ReturnsNotFound()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        course.Activate();
        var enrollment = CourseEnrollment.Create(Guid.NewGuid(), course.Id);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Course), course);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "video.mp4", 1);
        // Note: we do NOT add this lesson to the course.Lessons collection
        var progress = UserLessonProgress.Create(enrollment.Id, lesson.Id);
        enrollment.LessonProgress.Add(progress);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(enrollment.UserId, enrollment.CourseId, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new MarkLessonCompleteCommand(enrollment.UserId, enrollment.CourseId, lesson.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_LessonAlreadyComplete_ReturnsCorrectResult()
    {
        var (enrollment, lesson) = CreateEnrollmentWithLesson(Guid.NewGuid(), Guid.NewGuid(), true, lessonAlreadyCompleted: true);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(enrollment.UserId, enrollment.CourseId, default))
            .ReturnsAsync(enrollment);
        _enrollmentsMock
            .Setup(r => r.GetProgressAsync(enrollment.Id, lesson.Id, default))
            .ReturnsAsync(enrollment.LessonProgress.First());
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(enrollment.CourseId, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>()); // no final exam

        var result = await _handler.HandleAsync(new MarkLessonCompleteCommand(enrollment.UserId, enrollment.CourseId, lesson.Id));

        Assert.True(result.IsSuccess);
        var value = Assert.IsType<MarkLessonCompleteResult>(result.Value);
        Assert.True(value.LessonWasAlreadyComplete);
        Assert.Equal(1, value.CompletedLessons);
        Assert.Equal(1, value.TotalRequiredLessons);
    }

    [Fact]
    public async Task HandleAsync_CompletesRequiredLesson_UpdatesProgressAndDoesNotCompleteCourseWhenExamExists()
    {
        var (enrollment, lesson) = CreateEnrollmentWithLesson(Guid.NewGuid(), Guid.NewGuid(), true, lessonAlreadyCompleted: false);
        // Add an optional lesson to make sure we have at least one required lesson (the lesson is required)
        var optionalLesson = Lesson.Create(enrollment.CourseId, "Opcional", LessonType.Pdf, "pdf.pdf", 2, false);
        enrollment.Course.Lessons.Add(optionalLesson);

        // Simulate existing final exam (has questions)
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(enrollment.CourseId, default))
            .ReturnsAsync([QuizQuestion.CreateCourseExam(enrollment.CourseId, "Pregunta de examen", 70, 3, 1)]);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(enrollment.UserId, enrollment.CourseId, default))
            .ReturnsAsync(enrollment);
        _enrollmentsMock
            .Setup(r => r.GetProgressAsync(enrollment.Id, lesson.Id, default))
            .ReturnsAsync(enrollment.LessonProgress.First());

        var result = await _handler.HandleAsync(new MarkLessonCompleteCommand(enrollment.UserId, enrollment.CourseId, lesson.Id));

        Assert.True(result.IsSuccess);
        var value = Assert.IsType<MarkLessonCompleteResult>(result.Value);
        Assert.False(value.LessonWasAlreadyComplete);
        Assert.Equal(1, value.CompletedLessons);
        Assert.Equal(1, value.TotalRequiredLessons);
        Assert.False(value.CourseCompleted); // Because exam exists
        _enrollmentsMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CompletesLastRequiredLesson_NoExam_MarksCourseCompleted()
    {
        var (enrollment, lesson) = CreateEnrollmentWithLesson(Guid.NewGuid(), Guid.NewGuid(), true, lessonAlreadyCompleted: false);
        // No other lessons, so this is the only required lesson

        // No final exam
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(enrollment.CourseId, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(enrollment.UserId, enrollment.CourseId, default))
            .ReturnsAsync(enrollment);
        _enrollmentsMock
            .Setup(r => r.GetProgressAsync(enrollment.Id, lesson.Id, default))
            .ReturnsAsync(enrollment.LessonProgress.First());

        var result = await _handler.HandleAsync(new MarkLessonCompleteCommand(enrollment.UserId, enrollment.CourseId, lesson.Id));

        Assert.True(result.IsSuccess);
        var value = Assert.IsType<MarkLessonCompleteResult>(result.Value);
        Assert.False(value.LessonWasAlreadyComplete);
        Assert.Equal(1, value.CompletedLessons);
        Assert.Equal(1, value.TotalRequiredLessons);
        Assert.True(value.CourseCompleted); // Because it was last required and no exam
        _enrollmentsMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}