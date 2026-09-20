using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Tests.Unit;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class GetCourseExamHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly GetCourseExamHandler _handler;

    public GetCourseExamHandlerTests() =>
        _handler = new GetCourseExamHandler(_quizzesMock.Object, _enrollmentsMock.Object);

    // ── Fixture helpers ──────────────────────────────────────────────────────

    private static (Course course, CourseEnrollment enrollment) CreateActiveEnrollment(Guid userId)
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        course.Activate();
        var enrollment = CourseEnrollment.Create(userId, course.Id);
        Helpers.SetPrivate(enrollment, nameof(CourseEnrollment.Course), course);
        return (course, enrollment);
    }

    private static void MarkLessonComplete(CourseEnrollment enrollment, Guid lessonId)
    {
        var progress = UserLessonProgress.Create(enrollment.Id, lessonId);
        progress.MarkComplete();
        enrollment.LessonProgress.Add(progress);
    }

    // ── 1. Basic validation ──────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetCourseExamQuery(Guid.Empty, Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_EmptyCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetCourseExamQuery(Guid.NewGuid(), Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── 2. Forbidden paths ───────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NotEnrolled_ReturnsForbidden()
    {
        var courseId = Guid.NewGuid();

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(It.IsAny<Guid>(), courseId, default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(new GetCourseExamQuery(Guid.NewGuid(), courseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InactiveEnrollment_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        enrollment.Abandon();

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new GetCourseExamQuery(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_MissingRequiredLesson_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true);
        course.Lessons.Add(lesson);
        // Not completed

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new GetCourseExamQuery(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.Contains("examen final", result.Error);
        _quizzesMock.Verify(r => r.GetQuestionsByCourseAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AllRequiredLessonsCompleted_DoesNotBlock()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true);
        course.Lessons.Add(lesson);
        MarkLessonComplete(enrollment, lesson.Id);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(course.Id, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        var result = await _handler.HandleAsync(new GetCourseExamQuery(userId, course.Id));

        Assert.True(result.IsSuccess);
    }

    // ── 3. Empty result ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NoQuestions_ReturnsEmptyListNotError()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        // No lessons at all → nothing required, gating passes trivially

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(course.Id, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        var result = await _handler.HandleAsync(new GetCourseExamQuery(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    // ── 4. Happy path — full DTO mapping ─────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsMappedDtoWithOptionsInOrderAndNoAnswerLeak()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true);
        course.Lessons.Add(lesson);
        MarkLessonComplete(enrollment, lesson.Id);

        var question = QuizQuestion.CreateCourseExam(course.Id, "Pregunta final", passScore: 70m, maxAttempts: 2, orderIndex: 1, isRequired: false);
        var optionB = QuizOption.Create(question.Id, "Incorrecta", isCorrect: false, orderIndex: 2);
        var optionA = QuizOption.Create(question.Id, "Correcta", isCorrect: true, orderIndex: 1);
        question.AddOption(optionB);
        question.AddOption(optionA);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(course.Id, default))
            .ReturnsAsync([question]);

        var result = await _handler.HandleAsync(new GetCourseExamQuery(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var dto = result.Value[0];

        Assert.Equal(question.Id, dto.Id);
        Assert.Equal("Pregunta final", dto.QuestionText);
        Assert.Equal((int)QuizType.CourseExam, dto.Type);
        Assert.False(dto.IsRequired);
        Assert.Equal(70m, dto.PassScore);
        Assert.Equal(2, dto.MaxAttempts);
        Assert.Equal(1, dto.OrderIndex);
        Assert.Null(dto.LessonId);
        Assert.Equal(course.Id, dto.CourseId);

        Assert.Equal(2, dto.Options.Count);
        Assert.Equal(optionA.Id, dto.Options[0].Id);
        Assert.Equal("Correcta", dto.Options[0].OptionText);
        Assert.Equal(optionB.Id, dto.Options[1].Id);
        Assert.Equal("Incorrecta", dto.Options[1].OptionText);

        // QuizOptionDto exposes no IsCorrect property — the correct answer is never
        // returned to the caller of this student-facing query.
        var optionType = dto.Options[0].GetType();
        Assert.Null(optionType.GetProperty("IsCorrect"));
    }
}
