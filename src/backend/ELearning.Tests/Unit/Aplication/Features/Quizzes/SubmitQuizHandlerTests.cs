using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Tests.Unit;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class SubmitQuizHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly SubmitQuizHandler _handler;

    public SubmitQuizHandlerTests() =>
        _handler = new SubmitQuizHandler(
            _enrollmentsMock.Object, _lessonsMock.Object, _coursesMock.Object, _quizzesMock.Object);

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

    private static (QuizQuestion question, QuizOption correct, QuizOption incorrect) BuildLessonQuestion(
        Guid lessonId, decimal passScore = 60m, int maxAttempts = 3, int orderIndex = 1)
    {
        var question = QuizQuestion.CreatePerLesson(lessonId, "¿2+2?", passScore, maxAttempts, orderIndex);
        var correct = QuizOption.Create(question.Id, "4", true, 1);
        var incorrect = QuizOption.Create(question.Id, "5", false, 2);
        return (question, correct, incorrect);
    }

    private static (QuizQuestion question, QuizOption correct, QuizOption incorrect) BuildExamQuestion(
        Guid courseId, decimal passScore = 70m, int maxAttempts = 3, int orderIndex = 1)
    {
        var question = QuizQuestion.CreateCourseExam(courseId, "Pregunta final", passScore, maxAttempts, orderIndex);
        var correct = QuizOption.Create(question.Id, "Correcta", true, 1);
        var incorrect = QuizOption.Create(question.Id, "Incorrecta", false, 2);
        return (question, correct, incorrect);
    }

    private void SetupOptionLookup(params QuizOption[] options)
    {
        foreach (var option in options)
        {
            _quizzesMock
                .Setup(r => r.GetOptionByIdAsync(option.Id, default))
                .ReturnsAsync(option);
        }
    }

    // ── 1. Basic validation ──────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.Empty, Guid.NewGuid(), null, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_EmptySelectedOptionIds_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.NewGuid(), Guid.NewGuid(), null, []));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_NullSelectedOptionIds_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.NewGuid(), Guid.NewGuid(), null, null!));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_NoLessonOrCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.NewGuid(), null, null, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── 2. Lesson-context path ───────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_LessonNotFound_ReturnsNotFound()
    {
        _lessonsMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Lesson?)null);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.NewGuid(), Guid.NewGuid(), null, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_LessonContext_NotEnrolled_ReturnsForbidden()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "video.mp4", 1);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(It.IsAny<Guid>(), course.Id, default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.NewGuid(), lesson.Id, null, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    // ── 3. Course-context path ───────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_CourseContext_CourseNotFound_ReturnsNotFound()
    {
        _coursesMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Course?)null);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.NewGuid(), null, Guid.NewGuid(), [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_CourseContext_NotEnrolled_ReturnsForbidden()
    {
        var course = Course.Create("Curso", "Desc", null, Guid.NewGuid(), isGlobal: false);
        course.Activate();

        _coursesMock.Setup(r => r.GetByIdAsync(course.Id, default)).ReturnsAsync(course);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(It.IsAny<Guid>(), course.Id, default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(Guid.NewGuid(), null, course.Id, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    // ── 4. Inactive enrollment ───────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_InactiveEnrollment_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        enrollment.Abandon();

        _coursesMock.Setup(r => r.GetByIdAsync(course.Id, default)).ReturnsAsync(course);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, null, course.Id, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    // ── 5. Lesson-context prerequisite gating ────────────────────────────────

    [Fact]
    public async Task HandleAsync_LessonContext_MissingRequiredLessonBefore_ReturnsForbiddenWithPrerequisiteMessage()
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

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson2.Id, null, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.Contains("lecciones requeridas previas", result.Error);
    }

    [Fact]
    public async Task HandleAsync_LessonContext_PrerequisiteCompleted_DoesNotBlockOnGating()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);

        var lesson1 = Lesson.Create(course.Id, "Lección 1", LessonType.Video, "v1.mp4", 1, isRequired: true);
        var lesson2 = Lesson.Create(course.Id, "Lección 2", LessonType.Video, "v2.mp4", 2, isRequired: true);
        course.Lessons.Add(lesson1);
        course.Lessons.Add(lesson2);
        MarkLessonComplete(enrollment, lesson1.Id);

        var (question, correct, _) = BuildLessonQuestion(lesson2.Id);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson2.Id, default)).ReturnsAsync(lesson2);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson2.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson2.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(correct);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson2.Id, null, [correct.Id]));

        Assert.True(result.IsSuccess);
    }

    // ── 6. Course-context (final exam) gating ────────────────────────────────

    [Fact]
    public async Task HandleAsync_CourseContext_MissingRequiredLesson_ReturnsForbiddenWithFinalExamMessage()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true);
        course.Lessons.Add(lesson);
        // Not completed

        _coursesMock.Setup(r => r.GetByIdAsync(course.Id, default)).ReturnsAsync(course);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, null, course.Id, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.Contains("examen final", result.Error);
    }

    // ── 7. No questions ───────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NoQuestionsFound_ReturnsNotFound()
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

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [Guid.NewGuid()]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    // ── 8. Answer count mismatch ─────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SelectedOptionCountMismatch_ReturnsValidationFailure()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question1, correct1, _) = BuildLessonQuestion(lesson.Id, orderIndex: 1);
        var (question2, correct2, _) = BuildLessonQuestion(lesson.Id, orderIndex: 2);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question1, question2]);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct1.Id])); // only 1 answer for 2 questions

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── 9. Attempt-limit logic ────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_AlreadyPassed_ReturnsValidationFailure_NoMoreAttemptsNeeded()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question, correct, _) = BuildLessonQuestion(lesson.Id, passScore: 60m, maxAttempts: 3);
        var passedResult = UserQuizResult.Create(userId, lesson.Id, null, 1, 100m, 60m);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync(passedResult);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct.Id]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        Assert.Contains("Ya aprobaste", result.Error);
        _quizzesMock.Verify(r => r.CreateAttemptAsync(It.IsAny<UserQuizAttempt>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_MaxAttemptsReached_ReturnsValidationFailure()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question, correct, _) = BuildLessonQuestion(lesson.Id, passScore: 60m, maxAttempts: 3);
        var failedResult = UserQuizResult.Create(userId, lesson.Id, null, 3, 20m, 60m); // attemptNumber == maxAttempts, not passed

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync(failedResult);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct.Id]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        Assert.Contains("máximo de 3 intentos", result.Error);
    }

    [Fact]
    public async Task HandleAsync_FirstAttempt_NoLatestResult_AttemptNumberIsOne()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question, correct, _) = BuildLessonQuestion(lesson.Id, passScore: 60m, maxAttempts: 3);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(correct);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct.Id]));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.AttemptNumber);
    }

    [Fact]
    public async Task HandleAsync_HasAttemptsLeft_ProceedsAndIncrementsAttemptNumber()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question, correct, _) = BuildLessonQuestion(lesson.Id, passScore: 60m, maxAttempts: 3);
        var previousResult = UserQuizResult.Create(userId, lesson.Id, null, 1, 20m, 60m); // failed, attempt 1 of 3

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync(previousResult);
        SetupOptionLookup(correct);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct.Id]));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.AttemptNumber);
    }

    // ── 10. Invalid selected option ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SelectedOptionNotFound_ReturnsValidationFailure()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question, _, _) = BuildLessonQuestion(lesson.Id);
        var unknownOptionId = Guid.NewGuid();

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        _quizzesMock
            .Setup(r => r.GetOptionByIdAsync(unknownOptionId, default))
            .ReturnsAsync((QuizOption?)null);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [unknownOptionId]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_SelectedOptionBelongsToDifferentQuestion_ReturnsValidationFailure()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question, _, _) = BuildLessonQuestion(lesson.Id);
        // Option belongs to a completely different question
        var foreignOption = QuizOption.Create(Guid.NewGuid(), "Ajena", true, 1);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(foreignOption);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [foreignOption.Id]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── 11. Scoring ───────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_PartialCorrectAnswers_ComputesScoreAndFailsBelowPassScore()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (q1, correct1, incorrect1) = BuildLessonQuestion(lesson.Id, passScore: 60m, orderIndex: 1);
        var (q2, correct2, incorrect2) = BuildLessonQuestion(lesson.Id, passScore: 60m, orderIndex: 2);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([q1, q2]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(correct1, incorrect2);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct1.Id, incorrect2.Id])); // 1 of 2 correct

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Value.Score);
        Assert.Equal(1, result.Value.CorrectAnswers);
        Assert.Equal(2, result.Value.TotalQuestions);
        Assert.False(result.Value.IsPassed);
    }

    [Fact]
    public async Task HandleAsync_AllCorrectAnswers_ComputesFullScoreAndPasses()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (q1, correct1, _) = BuildLessonQuestion(lesson.Id, passScore: 60m, orderIndex: 1);
        var (q2, correct2, _) = BuildLessonQuestion(lesson.Id, passScore: 60m, orderIndex: 2);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([q1, q2]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(correct1, correct2);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct1.Id, correct2.Id]));

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value.Score);
        Assert.Equal(2, result.Value.CorrectAnswers);
        Assert.True(result.Value.IsPassed);
    }

    [Fact]
    public async Task HandleAsync_ScoreExactlyEqualsPassScore_IsPassedTrue()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        // 1 of 2 correct => score 50%; set passScore to exactly 50 to hit the boundary (score >= passScore)
        var (q1, correct1, _) = BuildLessonQuestion(lesson.Id, passScore: 50m, orderIndex: 1);
        var (q2, _, incorrect2) = BuildLessonQuestion(lesson.Id, passScore: 50m, orderIndex: 2);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([q1, q2]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(correct1, incorrect2);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct1.Id, incorrect2.Id]));

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Value.Score);
        Assert.Equal(50m, result.Value.PassScore);
        Assert.True(result.Value.IsPassed); // boundary: score == passScore should pass (>=)
    }

    // ── 12. TryComplete side effect on final exam ────────────────────────────

    [Fact]
    public async Task HandleAsync_CourseContext_PassedFinalExam_CompletesEnrollment()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true);
        course.Lessons.Add(lesson);
        MarkLessonComplete(enrollment, lesson.Id); // required lesson already done, so exam gating passes

        var (question, correct, _) = BuildExamQuestion(course.Id, passScore: 70m);

        _coursesMock.Setup(r => r.GetByIdAsync(course.Id, default)).ReturnsAsync(course);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(course.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestCourseExamResultAsync(userId, course.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(correct);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, null, course.Id, [correct.Id]));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsPassed);
        Assert.True(enrollment.IsCompleted);
        Assert.NotNull(enrollment.CompletedAt);
    }

    [Fact]
    public async Task HandleAsync_CourseContext_FailedFinalExam_DoesNotCompleteEnrollment()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true);
        course.Lessons.Add(lesson);
        MarkLessonComplete(enrollment, lesson.Id);

        var (question, _, incorrect) = BuildExamQuestion(course.Id, passScore: 70m);

        _coursesMock.Setup(r => r.GetByIdAsync(course.Id, default)).ReturnsAsync(course);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(course.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestCourseExamResultAsync(userId, course.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(incorrect);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, null, course.Id, [incorrect.Id]));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsPassed);
        Assert.False(enrollment.IsCompleted);
        Assert.Null(enrollment.CompletedAt);
    }

    // ── 13. Repository interaction verification ─────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidSubmission_CreatesOneAttemptPerQuestionAndSavesOnce()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (q1, correct1, _) = BuildLessonQuestion(lesson.Id, orderIndex: 1);
        var (q2, correct2, _) = BuildLessonQuestion(lesson.Id, orderIndex: 2);

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([q1, q2]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync((UserQuizResult?)null);
        SetupOptionLookup(correct1, correct2);
        _quizzesMock
            .Setup(r => r.CreateAttemptAsync(It.IsAny<UserQuizAttempt>(), default))
            .ReturnsAsync((UserQuizAttempt a, CancellationToken _) => a);
        _quizzesMock
            .Setup(r => r.CreateResultAsync(It.IsAny<UserQuizResult>(), default))
            .ReturnsAsync((UserQuizResult r, CancellationToken _) => r);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct1.Id, correct2.Id]));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.CreateAttemptAsync(It.IsAny<UserQuizAttempt>(), default), Times.Exactly(2));
        _quizzesMock.Verify(r => r.CreateResultAsync(It.IsAny<UserQuizResult>(), default), Times.Once);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    // ── 14. Response DTO fields ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidSubmission_ReturnsCompleteDtoFields()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: false);
        course.Lessons.Add(lesson);

        var (question, correct, _) = BuildLessonQuestion(lesson.Id, passScore: 60m, maxAttempts: 5);
        var previousResult = UserQuizResult.Create(userId, lesson.Id, null, 1, 20m, 60m); // 1 prior failed attempt

        _lessonsMock.Setup(r => r.GetByIdAsync(lesson.Id, default)).ReturnsAsync(lesson);
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByLessonAsync(lesson.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestLessonResultAsync(userId, lesson.Id, default))
            .ReturnsAsync(previousResult);
        SetupOptionLookup(correct);

        var result = await _handler.HandleAsync(
            new SubmitQuizCommand(userId, lesson.Id, null, [correct.Id]));

        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.Equal(100m, dto.Score);
        Assert.True(dto.IsPassed);
        Assert.Equal(60m, dto.PassScore);
        Assert.Equal(1, dto.TotalQuestions);
        Assert.Equal(1, dto.CorrectAnswers);
        Assert.Equal(2, dto.AttemptNumber); // previous attempt was #1
        Assert.Equal(5, dto.MaxAttempts);
        Assert.False(string.IsNullOrWhiteSpace(dto.Feedback));
    }
}
