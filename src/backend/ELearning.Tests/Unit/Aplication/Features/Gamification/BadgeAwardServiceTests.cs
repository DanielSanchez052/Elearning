using ELearning.Application.Features.Gamification.Rules;
using ELearning.Application.Features.Gamification.Services;
using ELearning.Application.Features.Notifications.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

public class BadgeAwardServiceTests
{
    private readonly Mock<IBadgeRepository> _badgesMock = new();
    private readonly Mock<INotificationPublisher> _publisherMock = new();
    private readonly Mock<ILogger<BadgeAwardService>> _loggerMock = new();
    private readonly BadgeAwardService _service;

    public BadgeAwardServiceTests() =>
        _service = new BadgeAwardService(
            _badgesMock.Object,
            new ILoginBadgeRule[] { new FirstLoginRule() },
            new ICourseCompletionBadgeRule[] { new CourseCompletedRule() },
            new IExamPassedBadgeRule[] { new SpeedsterRule() },
            _publisherMock.Object,
            _loggerMock.Object);

    private static User BuildUser() => User.Create("Test", "test@test.com", "hash", countryId: 1);

    // ── Badge-earned notification ────────────────────────────────────────────

    [Fact]
    public async Task NewAward_PublishesBadgeEarnedNotificationForTheNewUserBadge_ThenSavesOnce()
    {
        var user = BuildUser();
        var badge = GamificationTestHelpers.BuildBadge(1, BadgeCode.LoginFirst, "Primer Inicio de Sesión");
        UserBadge? inserted = null;
        var calls = new List<string>();

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.LoginFirst, default)).ReturnsAsync(badge);
        _badgesMock.Setup(r => r.HasBadgeAsync(user.Id, badge.Id, null, default)).ReturnsAsync(false);
        _badgesMock
            .Setup(r => r.TryAddAsync(It.IsAny<UserBadge>(), default))
            .Callback((UserBadge ub, CancellationToken _) => inserted = ub)
            .ReturnsAsync(true);
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<Guid?>(), default))
            .Callback(() => calls.Add("publish"))
            .Returns(Task.CompletedTask);
        _badgesMock.Setup(r => r.SaveChangesAsync(default)).Callback(() => calls.Add("save")).Returns(Task.CompletedTask);

        await _service.OnUserLoggedInAsync(user);

        Assert.NotNull(inserted);
        _publisherMock.Verify(p => p.PublishAsync(
            user.Id,
            NotificationType.BadgeEarned,
            It.Is<string>(t => t.Contains("Primer Inicio de Sesión")),
            It.Is<string>(m => !string.IsNullOrWhiteSpace(m)),
            inserted!.Id,
            default), Times.Once);
        Assert.Equal(["publish", "save"], calls);
    }

    [Fact]
    public async Task AlreadyOwned_PublishesNothingAndDoesNotSave()
    {
        var user = BuildUser();
        var badge = GamificationTestHelpers.BuildBadge(1, BadgeCode.LoginFirst);

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.LoginFirst, default)).ReturnsAsync(badge);
        _badgesMock.Setup(r => r.HasBadgeAsync(user.Id, badge.Id, null, default)).ReturnsAsync(true);

        await _service.OnUserLoggedInAsync(user);

        VerifyNothingPublishedOrSaved();
    }

    [Fact]
    public async Task LostInsertRace_PublishesNothingAndDoesNotSave()
    {
        var user = BuildUser();
        var badge = GamificationTestHelpers.BuildBadge(1, BadgeCode.LoginFirst);

        _badgesMock.Setup(r => r.GetByCodeAsync(BadgeCode.LoginFirst, default)).ReturnsAsync(badge);
        _badgesMock.Setup(r => r.HasBadgeAsync(user.Id, badge.Id, null, default)).ReturnsAsync(false);
        _badgesMock.Setup(r => r.TryAddAsync(It.IsAny<UserBadge>(), default)).ReturnsAsync(false);

        await _service.OnUserLoggedInAsync(user);

        VerifyNothingPublishedOrSaved();
    }

    [Theory]
    [InlineData(BadgeCode.LoginFirst, "Primer Inicio de Sesión")]
    [InlineData(BadgeCode.CourseDone, "Curso Completado")]
    [InlineData(BadgeCode.Speedster, "Velocista")]
    public void BuildNotificationCopy_EveryBadge_HasSpanishTitleWithNameAndAMessage(BadgeCode code, string name)
    {
        var badge = GamificationTestHelpers.BuildBadge(1, code, name);

        var (title, message) = BadgeAwardService.BuildNotificationCopy(badge);

        Assert.Contains(name, title);
        Assert.True(title.Length <= 150, "notifications.title is varchar(150)");
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    private void VerifyNothingPublishedOrSaved()
    {
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _badgesMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Best-effort contract (all 3 triggers: login, course completion, exam) ──

    private static CourseEnrollment BuildEnrollment() =>
        CourseEnrollment.Create(Guid.NewGuid(), Guid.NewGuid());

    private static UserQuizResult BuildPassedExamResult()
    {
        var startedAt = new DateTime(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc);
        var result = UserQuizResult.Create(
            userId: Guid.NewGuid(),
            lessonId: null,
            courseId: Guid.NewGuid(),
            attemptNumber: 1,
            score: 100m,
            passScore: 70m,
            startedAt: startedAt);
        GamificationTestHelpers.SetPrivate(result, nameof(UserQuizResult.CompletedAt), startedAt + TimeSpan.FromMinutes(5));
        return result;
    }

    private void VerifyErrorLogged(Times times) =>
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times);

    [Fact]
    public async Task OnUserLoggedInAsync_RepositoryThrows_ReturnsEmptyAndLogsErrorInsteadOfThrowing()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var result = await _service.OnUserLoggedInAsync(BuildUser());

        Assert.Empty(result);
        VerifyErrorLogged(Times.Once());
    }

    [Fact]
    public async Task OnUserLoggedInAsync_Cancelled_PropagatesCancellationWithoutLoggingAnError()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => _service.OnUserLoggedInAsync(BuildUser()));

        VerifyErrorLogged(Times.Never());
    }

    [Fact]
    public async Task OnCourseCompletedAsync_RepositoryThrows_ReturnsEmptyAndLogsErrorInsteadOfThrowing()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var result = await _service.OnCourseCompletedAsync(BuildEnrollment());

        Assert.Empty(result);
        VerifyErrorLogged(Times.Once());
    }

    [Fact]
    public async Task OnCourseCompletedAsync_Cancelled_PropagatesCancellationWithoutLoggingAnError()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => _service.OnCourseCompletedAsync(BuildEnrollment()));

        VerifyErrorLogged(Times.Never());
    }

    [Fact]
    public async Task OnCourseExamPassedAsync_RepositoryThrows_ReturnsEmptyAndLogsErrorInsteadOfThrowing()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var result = await _service.OnCourseExamPassedAsync(BuildPassedExamResult());

        Assert.Empty(result);
        VerifyErrorLogged(Times.Once());
    }

    [Fact]
    public async Task OnCourseExamPassedAsync_Cancelled_PropagatesCancellationWithoutLoggingAnError()
    {
        _badgesMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => _service.OnCourseExamPassedAsync(BuildPassedExamResult()));

        VerifyErrorLogged(Times.Never());
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
