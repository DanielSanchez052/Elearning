using ELearning.Application.Features.Notifications.Messages;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Tests.Unit.Aplication.Features.Gamification;

namespace ELearning.Tests.Unit.Aplication.Features.Notifications;

public class NotificationMessagesTests
{
    private const int TitleMaxLength = 150; // notifications.title is varchar(150)

    [Theory]
    [InlineData(BadgeCode.LoginFirst, "Primer Inicio de Sesión")]
    [InlineData(BadgeCode.CourseDone, "Curso Completado")]
    [InlineData(BadgeCode.Speedster, "Velocista")]
    public void BadgeEarned_EveryBadge_HasSpanishTitleWithNameAndABody(BadgeCode code, string name)
    {
        var userBadgeId = Guid.NewGuid();
        var message = new BadgeEarnedMessage(GamificationTestHelpers.BuildBadge(1, code, name), userBadgeId);

        Assert.Equal(NotificationType.BadgeEarned, message.Type);
        Assert.Equal(userBadgeId, message.ReferenceId);
        Assert.Contains(name, message.Title);
        Assert.True(message.Title.Length <= TitleMaxLength);
        Assert.False(string.IsNullOrWhiteSpace(message.Body));
    }

    [Fact]
    public void CourseAssigned_PointsToTheCourseAndNamesIt()
    {
        var course = Course.Create("Excel Avanzado", null, null, Guid.NewGuid(), isGlobal: false);

        var message = new CourseAssignedMessage(course);

        Assert.Equal(NotificationType.CourseAssigned, message.Type);
        Assert.Equal(course.Id, message.ReferenceId);
        Assert.Contains("Excel Avanzado", message.Body);
        Assert.True(message.Title.Length <= TitleMaxLength);
    }

    [Fact]
    public void CourseUnassigned_PointsToTheCourseAndNamesIt()
    {
        var course = Course.Create("Excel Avanzado", null, null, Guid.NewGuid(), isGlobal: false);

        var message = new CourseUnassignedMessage(course);

        Assert.Equal(NotificationType.CourseUnassigned, message.Type);
        Assert.Equal(course.Id, message.ReferenceId);
        Assert.Contains("Excel Avanzado", message.Body);
        Assert.True(message.Title.Length <= TitleMaxLength);
    }
}
