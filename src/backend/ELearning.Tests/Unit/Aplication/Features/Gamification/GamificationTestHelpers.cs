using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

internal static class GamificationTestHelpers
{
    /// <summary>
    /// Badge.Id is a private-set identity column (assigned by EF on insert).
    /// Tests need a fixed, known Id to assert against, so it's set via
    /// reflection — same approach as CertificateTestHelpers.SetPrivate.
    /// </summary>
    public static Badge BuildBadge(int id, BadgeCode code, string? name = null)
    {
        var badge = Badge.Create(code.ToString(), name ?? code.ToString());
        SetPrivate(badge, "Id", id);
        return badge;
    }

    /// <summary>
    /// UserBadge with its Badge (and, optionally, Course) navigation properties
    /// injected via reflection, the way EF's Include() would populate them.
    /// </summary>
    public static UserBadge BuildUserBadge(
        Guid userId,
        string badgeCode,
        string badgeName,
        Guid? courseId = null,
        string? courseTitle = null)
    {
        var badge = Badge.Create(badgeCode, badgeName, description: null);
        SetPrivate(badge, "Id", 1);

        var userBadge = UserBadge.Create(userId, badge.Id, courseId);
        SetPrivate(userBadge, "Badge", badge);

        if (courseId is not null)
        {
            var course = Course.Create(
                title: courseTitle ?? "Curso de prueba",
                description: null,
                thumbnailUrl: null,
                createdBy: Guid.NewGuid(),
                isGlobal: true);
            SetPrivate(userBadge, "Course", course);
        }

        return userBadge;
    }

    public static void SetPrivate(object obj, string propertyName, object? value)
    {
        var prop = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }
}
