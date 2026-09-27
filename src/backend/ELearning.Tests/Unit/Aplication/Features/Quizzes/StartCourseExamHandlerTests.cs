using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Tests.Unit;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class StartCourseExamHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly StartCourseExamHandler _handler;

    public StartCourseExamHandlerTests() =>
        _handler = new StartCourseExamHandler(_quizzesMock.Object, _enrollmentsMock.Object);

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

    private static QuizQuestion BuildExamQuestion(Guid courseId, int maxAttempts = 3, int orderIndex = 1)
    {
        var question = QuizQuestion.CreateCourseExam(courseId, "Pregunta final", 70m, maxAttempts, orderIndex);
        question.Options.Add(QuizOption.Create(question.Id, "Incorrecta", false, 2));
        question.Options.Add(QuizOption.Create(question.Id, "Correcta", true, 1));
        return question;
    }

    /// <summary>
    /// Enrolled, active, all required lessons done, one exam question with the given attempt limit,
    /// and the given latest exam result (null = never attempted).
    /// </summary>
    private (Course course, QuizQuestion question) SetupEligibleStudent(
        Guid userId, UserQuizResult? latestResult = null, int maxAttempts = 3)
    {
        var (course, enrollment) = CreateActiveEnrollment(userId);
        var lesson = Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true);
        course.Lessons.Add(lesson);
        MarkLessonComplete(enrollment, lesson.Id);

        var question = BuildExamQuestion(course.Id, maxAttempts);

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(course.Id, default))
            .ReturnsAsync([question]);
        _quizzesMock
            .Setup(r => r.GetLatestCourseExamResultAsync(userId, course.Id, default))
            .ReturnsAsync(latestResult);

        return (course, question);
    }

    /// <summary>
    /// Makes the session repository behave like the real one: TryAdd stores the session,
    /// GetOpen returns it only while it is still open.
    /// </summary>
    private Func<ExamSession?> UseInMemorySessionStore(ExamSession? initial = null)
    {
        ExamSession? stored = initial;
        _quizzesMock
            .Setup(r => r.GetOpenExamSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(() => stored is { IsOpen: true } ? stored : null);
        _quizzesMock
            .Setup(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default))
            .Callback((ExamSession s, CancellationToken _) => stored = s)
            .ReturnsAsync(ExamSessionInsertResult.Inserted);
        return () => stored;
    }

    // ── 1. Basic validation ──────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new StartCourseExamCommand(Guid.Empty, Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_EmptyCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new StartCourseExamCommand(Guid.NewGuid(), Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── 2. Enrollment gating (mirrors GetCourseExamHandler) ──────────────────

    [Fact]
    public async Task HandleAsync_NotEnrolled_ReturnsForbidden()
    {
        var courseId = Guid.NewGuid();
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(It.IsAny<Guid>(), courseId, default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(Guid.NewGuid(), courseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Never);
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

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_MissingRequiredLesson_ReturnsForbiddenWithFinalExamMessage()
    {
        var userId = Guid.NewGuid();
        var (course, enrollment) = CreateActiveEnrollment(userId);
        course.Lessons.Add(Lesson.Create(course.Id, "Lección", LessonType.Video, "v.mp4", 1, isRequired: true));
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, course.Id, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
        Assert.Contains("examen final", result.Error);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoQuestions_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        _quizzesMock
            .Setup(r => r.GetQuestionsByCourseAsync(course.Id, default))
            .ReturnsAsync(Array.Empty<QuizQuestion>());

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Never);
    }

    // ── 3. Attempt rules (mirrors SubmitQuizHandler) ─────────────────────────

    [Fact]
    public async Task HandleAsync_AlreadyPassed_ReturnsValidationFailureAndCreatesNoSession()
    {
        var userId = Guid.NewGuid();
        var passed = UserQuizResult.Create(userId, null, Guid.NewGuid(), 1, 100m, 70m);
        var (course, _) = SetupEligibleStudent(userId, passed);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        Assert.Contains("Ya aprobaste", result.Error);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoAttemptsRemaining_ReturnsValidationFailureAndCreatesNoSession()
    {
        var userId = Guid.NewGuid();
        var lastFailed = UserQuizResult.Create(userId, null, Guid.NewGuid(), 3, 10m, 70m);
        var (course, _) = SetupEligibleStudent(userId, lastFailed, maxAttempts: 3);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        Assert.Contains("máximo de 3 intentos", result.Error);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Never);
    }

    // ── 4. Session creation / resume ─────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_FirstCall_CreatesSessionWithAttemptOne()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        var stored = UseInMemorySessionStore();

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        var created = stored();
        Assert.NotNull(created);
        Assert.Equal(userId, created!.UserId);
        Assert.Equal(course.Id, created.CourseId);
        Assert.Equal(1, created.AttemptNumber);
        Assert.Equal(created.Id, result.Value.SessionId);
        Assert.Equal(1, result.Value.AttemptNumber);
        Assert.Equal(created.StartedAt, result.Value.StartedAt);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SecondCallWhileSessionOpen_ReturnsSameSessionUnchanged()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        UseInMemorySessionStore();

        var first = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));
        Thread.Sleep(15);
        var second = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value.SessionId, second.Value.SessionId);
        Assert.Equal(first.Value.StartedAt, second.Value.StartedAt);
        Assert.Equal(first.Value.AttemptNumber, second.Value.AttemptNumber);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AfterFirstSessionSubmittedAndFailed_CreatesNewSessionWithAttemptTwo()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        var firstSession = ExamSession.Start(userId, course.Id, 1);
        firstSession.MarkSubmitted();
        var stored = UseInMemorySessionStore(firstSession);
        _quizzesMock
            .Setup(r => r.GetLatestCourseExamResultAsync(userId, course.Id, default))
            .ReturnsAsync(UserQuizResult.Create(userId, null, course.Id, 1, 10m, 70m));

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(firstSession.Id, result.Value.SessionId);
        Assert.Equal(2, result.Value.AttemptNumber);
        Assert.Equal(2, stored()!.AttemptNumber);
    }

    [Fact]
    public async Task HandleAsync_OpenSecondAttemptSession_IsResumedWithoutCreatingAnother()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        _quizzesMock
            .Setup(r => r.GetLatestCourseExamResultAsync(userId, course.Id, default))
            .ReturnsAsync(UserQuizResult.Create(userId, null, course.Id, 1, 10m, 70m));
        var openSecondAttempt = ExamSession.Start(userId, course.Id, 2);
        UseInMemorySessionStore(openSecondAttempt);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(openSecondAttempt.Id, result.Value.SessionId);
        Assert.Equal(openSecondAttempt.StartedAt, result.Value.StartedAt);
        Assert.Equal(2, result.Value.AttemptNumber);
        Assert.True(openSecondAttempt.IsOpen);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Never);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ResumedSession_ReturnsServerNowSoClientCanComputeElapsedTime()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        var openSession = ExamSession.Start(userId, course.Id, 1);
        Helpers.SetPrivate(openSession, nameof(ExamSession.StartedAt), DateTime.UtcNow.AddMinutes(-4));
        UseInMemorySessionStore(openSession);
        var before = DateTime.UtcNow;

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.ServerNow, before, DateTime.UtcNow);
        Assert.InRange(result.Value.ServerNow - result.Value.StartedAt, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(4.1));
    }

    [Fact]
    public async Task HandleAsync_ReturnsStudentSafeQuestionsWithOrderedOptions()
    {
        var userId = Guid.NewGuid();
        var (course, question) = SetupEligibleStudent(userId);
        UseInMemorySessionStore();

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value.Questions);
        Assert.Equal(question.Id, dto.Id);
        Assert.Equal(course.Id, dto.CourseId);
        Assert.Equal(new[] { 1, 2 }, dto.Options.Select(o => o.OrderIndex));
    }

    // ── 5. Concurrency and defensive paths ───────────────────────────────────

    [Fact]
    public async Task HandleAsync_ConcurrentStartWinsRace_ReturnsTheSessionCreatedByTheOtherRequest()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        var winner = ExamSession.Start(userId, course.Id, 1);
        _quizzesMock
            .SetupSequence(r => r.GetOpenExamSessionAsync(userId, course.Id, default))
            .ReturnsAsync((ExamSession?)null)
            .ReturnsAsync(winner);
        _quizzesMock
            .Setup(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default))
            .ReturnsAsync(ExamSessionInsertResult.OpenSessionRace);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(winner.Id, result.Value.SessionId);
        Assert.Equal(winner.StartedAt, result.Value.StartedAt);
    }

    [Fact]
    public async Task HandleAsync_InsertRejectedAndNoOpenSessionFound_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        _quizzesMock
            .Setup(r => r.GetOpenExamSessionAsync(userId, course.Id, default))
            .ReturnsAsync((ExamSession?)null);
        _quizzesMock
            .Setup(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default))
            .ReturnsAsync(ExamSessionInsertResult.OpenSessionRace);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_AttemptNumberCollision_RetriesOnceWithNextAttemptAndSucceeds()
    {
        // Bug case (R3-003): the computed attempt number collides with idx_exam_session_user_course_attempt
        // (e.g. a closed session already exists for it, with no open session to resume). The handler must
        // retry once with the next attempt number instead of returning a permanent Conflict.
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        _quizzesMock
            .Setup(r => r.GetOpenExamSessionAsync(userId, course.Id, default))
            .ReturnsAsync((ExamSession?)null);
        _quizzesMock
            .SetupSequence(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default))
            .ReturnsAsync(ExamSessionInsertResult.AttemptNumberCollision)
            .ReturnsAsync(ExamSessionInsertResult.Inserted);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.AttemptNumber);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_AttemptNumberCollisionPersistsAfterRetry_ReturnsConflict()
    {
        // Still colliding after the single retry is a genuinely unexpected state (not auto-recoverable):
        // returning Conflict here (instead of looping forever) is the correct, honest behavior.
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        _quizzesMock
            .Setup(r => r.GetOpenExamSessionAsync(userId, course.Id, default))
            .ReturnsAsync((ExamSession?)null);
        _quizzesMock
            .Setup(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default))
            .ReturnsAsync(ExamSessionInsertResult.AttemptNumberCollision);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
        _quizzesMock.Verify(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_AttemptNumberCollisionThenOpenSessionRaceOnRetry_ReturnsResumedSession()
    {
        // Edge case: the retry (next attempt number) itself loses a genuine concurrent-open-session
        // race. The handler should still resolve it the normal way instead of surfacing Conflict.
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        var winner = ExamSession.Start(userId, course.Id, 2);
        _quizzesMock
            .SetupSequence(r => r.GetOpenExamSessionAsync(userId, course.Id, default))
            .ReturnsAsync((ExamSession?)null)
            .ReturnsAsync(winner);
        _quizzesMock
            .SetupSequence(r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), default))
            .ReturnsAsync(ExamSessionInsertResult.AttemptNumberCollision)
            .ReturnsAsync(ExamSessionInsertResult.OpenSessionRace);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(winner.Id, result.Value.SessionId);
    }

    [Fact]
    public async Task HandleAsync_OpenSessionForOutdatedAttempt_ClosesItAndStartsCurrentAttempt()
    {
        var userId = Guid.NewGuid();
        var (course, _) = SetupEligibleStudent(userId);
        // Attempt 1 already has a result, yet its session was left open (should not happen normally).
        _quizzesMock
            .Setup(r => r.GetLatestCourseExamResultAsync(userId, course.Id, default))
            .ReturnsAsync(UserQuizResult.Create(userId, null, course.Id, 1, 10m, 70m));
        var staleSession = ExamSession.Start(userId, course.Id, 1);
        var stored = UseInMemorySessionStore(staleSession);

        var result = await _handler.HandleAsync(new StartCourseExamCommand(userId, course.Id));

        Assert.True(result.IsSuccess);
        Assert.False(staleSession.IsOpen);
        Assert.Equal(2, result.Value.AttemptNumber);
        Assert.Equal(2, stored()!.AttemptNumber);
        _quizzesMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
