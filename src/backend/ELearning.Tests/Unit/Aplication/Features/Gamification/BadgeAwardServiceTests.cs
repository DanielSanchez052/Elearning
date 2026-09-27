using ELearning.Application.Features.Gamification.Rules;
using ELearning.Application.Features.Gamification.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

public class BadgeAwardServiceTests
{
    private readonly Mock<IBadgeRepository> _badgesMock = new();
    private readonly BadgeAwardService _service;

    public BadgeAwardServiceTests() =>
        _service = new BadgeAwardService(
            _badgesMock.Object,
            new ILoginBadgeRule[] { new FirstLoginRule() },
            new ICourseCompletionBadgeRule[] { new CourseCompletedRule() });

    private static User BuildUser() => User.Create("Test", "test@test.com", "hash", countryId: 1);

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

    // ── OnCourseExamPassedAsync (Track A stub) ──────────────────────────────

    [Fact]
    public async Task OnCourseExamPassedAsync_IsANoOpStub_ReturnsEmptyWithoutTouchingTheRepository()
    {
        var result = UserQuizResult.Create(
            userId: Guid.NewGuid(),
            lessonId: null,
            courseId: Guid.NewGuid(),
            attemptNumber: 1,
            score: 100m,
            passScore: 70m);

        var awarded = await _service.OnCourseExamPassedAsync(result);

        Assert.Empty(awarded);
        _badgesMock.Verify(r => r.GetByCodeAsync(It.IsAny<BadgeCode>(), default), Times.Never);
    }
}
