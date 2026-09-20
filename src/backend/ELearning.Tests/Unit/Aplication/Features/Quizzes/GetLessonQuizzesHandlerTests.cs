using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Tests.Unit;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class GetLessonQuizzesHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly GetLessonQuizzesHandler _handler;

    public GetLessonQuizzesHandlerTests() =>
        _handler = new GetLessonQuizzesHandler(_quizzesMock.Object, _lessonsMock.Object, _enrollmentsMock.Object);

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
        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(Guid.Empty, Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_EmptyLessonId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(Guid.NewGuid(), Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── 2. Not found ──────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_LessonNotFound_ReturnsNotFound()
    {
        _lessonsMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Lesson?)null);

        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(Guid.NewGuid(), Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    // ── 3. Forbidden paths ───────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NotEnrolled_ReturnsForbidden()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(It.IsAny<Guid>(), course.Id, default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(Guid.NewGuid(), lesson.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_InactiveEnrollment_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        enrollment.Abandon();
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(userId, lesson.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_MissingRequiredLessonBefore_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);

        var lesson1 = Lesson.Create(course.Id, "Lección 1", LessonType.Video, "v1.mp4", 1, isRequired: true);
        var lesson2 = Lesson.Create(course.Id, "Lección 2", LessonType.Video, "v2.mp4", 2, isRequired: true);
        course.Lessons.Add(lesson1);
        course.Lessons.Add(lesson2);
        // lesson1 (required, orderIndex before lesson2) is NOT completed

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson2.Id, default)).ReturnsAsync(lesson2);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(userId, lesson2.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.Contains("lecciones requeridas previas", result.Error);
        _quizzesMock.Verify(r => r.GetQuestionsByLessonAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RequiredLessonBeforeCompleted_DoesNotBlock()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);

        var lesson1 = Lesson.Create(course.Id, "Lección 1", LessonType.Video, "v1.mp4", 1, isRequired: true);
        var lesson2 = Lesson.Create(course.Id, "Lección 2", LessonType.Video, "v2.mp4", 2, isRequired: true);
        course.Lessons.Add(lesson1);
        course.Lessons.Add(lesson2);
        MarkLessonComplete(enrollment, lesson1.Id);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson2.Id, default)).ReturnsAsync(lesson2);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson2.Id, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(userId, lesson2.Id));

        Assert.True(result.IsSuccess);
    }

    // ── 4. Empty result ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NoQuestions_ReturnsEmptyListNotError()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(userId, lesson.Id));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    // ── 5. Happy path — full DTO mapping ─────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsMappedDtoWithOptionsInOrderAndNoAnswerLeak()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var question = QuizQuestion.CreatePerLesson(lesson.Id, "¿2+2?", passScore: 60m, maxAttempts: 3, orderIndex: 1, isRequired: true);
        // Deliberately created out of order to verify the handler re-sorts by OrderIndex
        var optionB = QuizOption.Create(question.Id, "5", isCorrect: false, orderIndex: 2);
        var optionA = QuizOption.Create(question.Id, "4", isCorrect: true, orderIndex: 1);
        question.AddOption(optionB);
        question.AddOption(optionA);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);

        var result = await _handler.HandleAsync(new GetLessonQuizzesQuery(userId, lesson.Id));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var dto = result.Value[0];

        Assert.Equal(question.Id, dto.Id);
        Assert.Equal("¿2+2?", dto.QuestionText);
        Assert.Equal((int)QuizType.PerLesson, dto.Type);
        Assert.True(dto.IsRequired);
        Assert.Equal(60m, dto.PassScore);
        Assert.Equal(3, dto.MaxAttempts);
        Assert.Equal(1, dto.OrderIndex);
        Assert.Equal(lesson.Id, dto.LessonId);
        Assert.Null(dto.CourseId);

        Assert.Equal(2, dto.Options.Count);
        // Re-ordered by OrderIndex regardless of insertion order
        Assert.Equal(optionA.Id, dto.Options[0].Id);
        Assert.Equal("4", dto.Options[0].OptionText);
        Assert.Equal(1, dto.Options[0].OrderIndex);
        Assert.Equal(optionB.Id, dto.Options[1].Id);
        Assert.Equal("5", dto.Options[1].OptionText);
        Assert.Equal(2, dto.Options[1].OrderIndex);

        // QuizOptionDto has no IsCorrect property at all — the correct/incorrect
        // status of optionA/optionB above is never exposed through this DTO.
        var optionType = dto.Options[0].GetType();
        Assert.Null(optionType.GetProperty("IsCorrect"));
    }
}
