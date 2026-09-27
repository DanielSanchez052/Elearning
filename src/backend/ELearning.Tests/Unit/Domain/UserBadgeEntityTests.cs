using ELearning.Domain.Entities;

namespace ELearning.Tests.Unit.Domain;

public class UserBadgeEntityTests
{
    [Fact]
    public void Create_WithoutCourseId_LeavesCourseIdNull()
    {
        var userId = Guid.NewGuid();

        var userBadge = UserBadge.Create(userId, badgeId: 1);

        Assert.Equal(userId, userBadge.UserId);
        Assert.Equal(1, userBadge.BadgeId);
        Assert.Null(userBadge.CourseId);
    }

    [Fact]
    public void Create_WithCourseId_SetsCourseId()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var userBadge = UserBadge.Create(userId, badgeId: 2, courseId: courseId);

        Assert.Equal(courseId, userBadge.CourseId);
    }

    [Fact]
    public void Create_SetsObtainedAtToUtcNow()
    {
        var before = DateTime.UtcNow;

        var userBadge = UserBadge.Create(Guid.NewGuid(), badgeId: 1);

        Assert.True(userBadge.ObtainedAt >= before);
    }

    [Fact]
    public void Create_GeneratesNonEmptyId()
    {
        var userBadge = UserBadge.Create(Guid.NewGuid(), badgeId: 1);

        Assert.NotEqual(Guid.Empty, userBadge.Id);
    }
}
