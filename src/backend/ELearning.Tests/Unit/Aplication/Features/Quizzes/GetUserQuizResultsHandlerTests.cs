using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

public class GetUserQuizResultsHandlerTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly GetUserQuizResultsHandler _handler;

    public GetUserQuizResultsHandlerTests() =>
        _handler = new GetUserQuizResultsHandler(_quizzesMock.Object);

    // ── 1. Basic validation ──────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(Guid.Empty, Guid.NewGuid(), null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_NoLessonOrCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(Guid.NewGuid(), null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_EmptyLessonIdAndEmptyCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(Guid.NewGuid(), Guid.Empty, Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── 2. Empty result ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NoAttemptsYet_ReturnsEmptyListNotError()
    {
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        _quizzesMock
            .Setup(r => r.GetLessonAttemptsAsync(userId, lessonId, default))
            .ReturnsAsync(Array.Empty<UserQuizResult>());

        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(userId, lessonId, null));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    // ── 3. Branch selection: lesson vs course ────────────────────────────────

    [Fact]
    public async Task HandleAsync_LessonIdProvided_CallsGetLessonAttemptsAsync()
    {
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _quizzesMock
            .Setup(r => r.GetLessonAttemptsAsync(userId, lessonId, default))
            .ReturnsAsync(Array.Empty<UserQuizResult>());

        // Provide both LessonId and CourseId — LessonId takes priority per handler logic
        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(userId, lessonId, courseId));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.GetLessonAttemptsAsync(userId, lessonId, default), Times.Once);
        _quizzesMock.Verify(r => r.GetCourseExamAttemptsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OnlyCourseIdProvided_CallsGetCourseExamAttemptsAsync()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _quizzesMock
            .Setup(r => r.GetCourseExamAttemptsAsync(userId, courseId, default))
            .ReturnsAsync(Array.Empty<UserQuizResult>());

        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(userId, null, courseId));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.GetCourseExamAttemptsAsync(userId, courseId, default), Times.Once);
        _quizzesMock.Verify(r => r.GetLessonAttemptsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmptyLessonIdWithCourseId_FallsBackToCourseExamAttempts()
    {
        // LessonId is technically HasValue but equals Guid.Empty — handler treats this
        // as "no lesson filter" and falls through to the course branch.
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _quizzesMock
            .Setup(r => r.GetCourseExamAttemptsAsync(userId, courseId, default))
            .ReturnsAsync(Array.Empty<UserQuizResult>());

        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(userId, Guid.Empty, courseId));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.GetCourseExamAttemptsAsync(userId, courseId, default), Times.Once);
    }

    // ── 4. Happy path — full DTO mapping ─────────────────────────────────────

    [Fact]
    public async Task HandleAsync_LessonAttemptsExist_ReturnsMappedDtos()
    {
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        var attempt1 = UserQuizResult.Create(userId, lessonId, null, 1, 40m, 60m); // failed
        var attempt2 = UserQuizResult.Create(userId, lessonId, null, 2, 80m, 60m); // passed

        _quizzesMock
            .Setup(r => r.GetLessonAttemptsAsync(userId, lessonId, default))
            .ReturnsAsync([attempt1, attempt2]);

        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(userId, lessonId, null));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        var dto1 = result.Value[0];
        Assert.Equal(attempt1.AttemptNumber, dto1.AttemptNumber);
        Assert.Equal(attempt1.Score, dto1.Score);
        Assert.Equal(attempt1.IsPassed, dto1.IsPassed);
        Assert.False(dto1.IsPassed);
        Assert.Equal(attempt1.CompletedAt, dto1.CompletedAt);

        var dto2 = result.Value[1];
        Assert.Equal(attempt2.AttemptNumber, dto2.AttemptNumber);
        Assert.Equal(attempt2.Score, dto2.Score);
        Assert.True(dto2.IsPassed);
        Assert.Equal(attempt2.CompletedAt, dto2.CompletedAt);
    }

    [Fact]
    public async Task HandleAsync_CourseExamAttemptsExist_ReturnsMappedDtos()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var attempt = UserQuizResult.Create(userId, null, courseId, 1, 90m, 70m);

        _quizzesMock
            .Setup(r => r.GetCourseExamAttemptsAsync(userId, courseId, default))
            .ReturnsAsync([attempt]);

        var result = await _handler.HandleAsync(new GetUserQuizResultsQuery(userId, null, courseId));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var dto = result.Value[0];
        Assert.Equal(attempt.AttemptNumber, dto.AttemptNumber);
        Assert.Equal(attempt.Score, dto.Score);
        Assert.Equal(attempt.IsPassed, dto.IsPassed);
        Assert.Equal(attempt.CompletedAt, dto.CompletedAt);
    }
}
