using ELearning.Application.Features.Gamification.Rules;
using ELearning.Application.Features.Gamification.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

public class BadgeAwardServiceTests
{
    private readonly Mock<IBadgeRepository> _badgesMock = new();
    private readonly Mock<ILogger<BadgeAwardService>> _loggerMock = new();
    private readonly BadgeAwardService _service;

    public BadgeAwardServiceTests() =>
        _service = new BadgeAwardService(
            _badgesMock.Object,
            new ILoginBadgeRule[] { new FirstLoginRule() },
            new ICourseCompletionBadgeRule[] { new CourseCompletedRule() },
            new IExamPassedBadgeRule[] { new SpeedsterRule() },
            _loggerMock.Object);

    private static User BuildUser() => User.Create("Test", "test@test.com", "hash", countryId: 1);

    // ── Best-effort contract ─────────────────────────────────────────────────

    [Fact]
    public async Task AnyHook_RepositoryThrows_ReturnsEmptyAndLogsErrorInsteadOfThrowing()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var result = await _service.OnUserLoggedInAsync(BuildUser());

        Assert.Empty(result);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<InvalidOperationException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task AnyHook_Cancelled_PropagatesCancellationWithoutLoggingAnError()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => _service.OnUserLoggedInAsync(BuildUser()));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Never);
    }

    // ── OnUserLoggedInAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task OnUserLoggedInAsync_UserAlreadyHasBadge_DoesNotAttemptInsert()
    {
        var user = BuildUser();
        var badge = GamificationTestHelpers.BuildBadge(1, BadgeCode.LoginFirst);

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.LoginFirst, default)).ReturnsAsync(badge);
        _badgesMock.Setup(r => r.HasBadgeAsync(user.Id, badge.Id, null, default)).ReturnsAsync(true);

        var result = await _service.OnUserLoggedInAsync(user);

        Assert.Empty(result);
        _badgesMock.Verify(r => r.TryAddAsync(It.IsAny<UserBadge>(), default), Times.Never);
    }

    [Fact]
    public async Task OnUserLoggedInAsync_TryAddReturnsFalse_TreatedAsNotAwarded_NoCrash()
    {
        var user = BuildUser();
        var badge = GamificationTestHelpers.BuildBadge(1, BadgeCode.LoginFirst);

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.LoginFirst, default)).ReturnsAsync(badge);
        _badgesMock.Setup(r => r.HasBadgeAsync(user.Id, badge.Id, null, default)).ReturnsAsync(false);
        _badgesMock.Setup(r => r.TryAddAsync(It.IsAny<UserBadge>(), default)).ReturnsAsync(false);

        var result = await _service.OnUserLoggedInAsync(user);

        Assert.Empty(result);
    }

    [Fact]
    public async Task OnUserLoggedInAsync_BadgeCodeNotSeeded_ReturnsEmptyWithoutThrowing()
    {
        var user = BuildUser();

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.LoginFirst, default)).ReturnsAsync((Badge?)null);

        var result = await _service.OnUserLoggedInAsync(user);

        Assert.Empty(result);
        _badgesMock.Verify(r => r.HasBadgeAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid?>(), default), Times.Never);
    }

    [Fact]
    public async Task OnUserLoggedInAsync_NewBadge_ReturnsAwardedBadgeInResult()
    {
        var user = BuildUser();
        var badge = GamificationTestHelpers.BuildBadge(1, BadgeCode.LoginFirst, "Primer Inicio de Sesión");

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.LoginFirst, default)).ReturnsAsync(badge);
        _badgesMock.Setup(r => r.HasBadgeAsync(user.Id, badge.Id, null, default)).ReturnsAsync(false);
        _badgesMock.Setup(r => r.TryAddAsync(It.IsAny<UserBadge>(), default)).ReturnsAsync(true);

        var result = await _service.OnUserLoggedInAsync(user);

        var awarded = Assert.Single(result);
        Assert.Equal(badge.Id, awarded.BadgeId);
        Assert.Equal(BadgeCode.LoginFirst.ToString(), awarded.Code);
        Assert.Equal("Primer Inicio de Sesión", awarded.Name);
        Assert.Null(awarded.CourseId);
    }

    // ── OnCourseCompletedAsync ───────────────────────────────────────────────

    [Fact]
    public async Task OnCourseCompletedAsync_NewBadge_ScopesToEnrollmentCourseId()
    {
        var enrollment = CourseEnrollment.Create(Guid.NewGuid(), Guid.NewGuid());
        var badge = GamificationTestHelpers.BuildBadge(2, BadgeCode.CourseDone);

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.CourseDone, default)).ReturnsAsync(badge);
        _badgesMock
            .Setup(r => r.HasBadgeAsync(enrollment.UserId, badge.Id, enrollment.CourseId, default))
            .ReturnsAsync(false);
        _badgesMock.Setup(r => r.TryAddAsync(It.IsAny<UserBadge>(), default)).ReturnsAsync(true);

        var result = await _service.OnCourseCompletedAsync(enrollment);

        var awarded = Assert.Single(result);
        Assert.Equal(enrollment.CourseId, awarded.CourseId);
    }

    [Fact]
    public async Task OnCourseCompletedAsync_AlreadyAwardedForThisCourse_DoesNotAttemptInsert()
    {
        var enrollment = CourseEnrollment.Create(Guid.NewGuid(), Guid.NewGuid());
        var badge = GamificationTestHelpers.BuildBadge(2, BadgeCode.CourseDone);

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.CourseDone, default)).ReturnsAsync(badge);
        _badgesMock
            .Setup(r => r.HasBadgeAsync(enrollment.UserId, badge.Id, enrollment.CourseId, default))
            .ReturnsAsync(true);

        var result = await _service.OnCourseCompletedAsync(enrollment);

        Assert.Empty(result);
        _badgesMock.Verify(r => r.TryAddAsync(It.IsAny<UserBadge>(), default), Times.Never);
    }

    // ── OnCourseExamPassedAsync ──────────────────────────────────────────────

    private static UserQuizResult BuildCourseExamResult(DateTime? startedAt, int attemptNumber = 1) =>
        UserQuizResult.Create(
            userId: Guid.NewGuid(),
            lessonId: null,
            courseId: Guid.NewGuid(),
            attemptNumber: attemptNumber,
            score: 100m,
            passScore: 70m,
            startedAt: startedAt);

    [Fact]
    public async Task OnCourseExamPassedAsync_FastFirstAttempt_AwardsSpeedsterScopedToCourse()
    {
        var result = BuildCourseExamResult(DateTime.UtcNow.AddMinutes(-5));
        var badge = GamificationTestHelpers.BuildBadge(3, BadgeCode.Speedster, "Velocista");

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.Speedster, default)).ReturnsAsync(badge);
        _badgesMock
            .Setup(r => r.HasBadgeAsync(result.UserId, badge.Id, result.CourseId, default))
            .ReturnsAsync(false);
        _badgesMock.Setup(r => r.TryAddAsync(It.IsAny<UserBadge>(), default)).ReturnsAsync(true);

        var awarded = await _service.OnCourseExamPassedAsync(result);

        var dto = Assert.Single(awarded);
        Assert.Equal(BadgeCode.Speedster.ToString(), dto.Code);
        Assert.Equal(result.CourseId, dto.CourseId);
        _badgesMock.Verify(r => r.TryAddAsync(
            It.Is<UserBadge>(ub => ub.UserId == result.UserId && ub.BadgeId == badge.Id && ub.CourseId == result.CourseId),
            default), Times.Once);
    }

    [Fact]
    public async Task OnCourseExamPassedAsync_ResultNotEligible_ReturnsEmptyWithoutTouchingTheRepository()
    {
        // No StartedAt → unknown duration → SpeedsterRule proposes nothing.
        var result = BuildCourseExamResult(startedAt: null);

        var awarded = await _service.OnCourseExamPassedAsync(result);

        Assert.Empty(awarded);
        _badgesMock.Verify(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default), Times.Never);
    }

    [Fact]
    public async Task OnCourseExamPassedAsync_AlreadyHasSpeedsterForCourse_DoesNotAttemptInsert()
    {
        var result = BuildCourseExamResult(DateTime.UtcNow.AddMinutes(-5));
        var badge = GamificationTestHelpers.BuildBadge(3, BadgeCode.Speedster);

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.Speedster, default)).ReturnsAsync(badge);
        _badgesMock
            .Setup(r => r.HasBadgeAsync(result.UserId, badge.Id, result.CourseId, default))
            .ReturnsAsync(true);

        var awarded = await _service.OnCourseExamPassedAsync(result);

        Assert.Empty(awarded);
        _badgesMock.Verify(r => r.TryAddAsync(It.IsAny<UserBadge>(), default), Times.Never);
    }
}
