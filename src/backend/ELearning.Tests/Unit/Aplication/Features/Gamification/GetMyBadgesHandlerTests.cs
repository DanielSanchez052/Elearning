using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Gamification.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

public class GetMyBadgesHandlerTests
{
    private readonly Mock<IBadgeRepository> _badgesMock = new();
    private readonly GetMyBadgesHandler _handler;

    public GetMyBadgesHandlerTests() =>
        _handler = new GetMyBadgesHandler(_badgesMock.Object);

    [Fact]
    public async Task HandleAsync_UserWithNoBadges_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _badgesMock
            .Setup(r => r.GetByUserAsync(userId, default))
            .ReturnsAsync(Array.Empty<UserBadge>());

        var result = await _handler.HandleAsync(new GetMyBadgesQuery(userId));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_CourseAgnosticBadge_MapsWithNullCourseFields()
    {
        var userId = Guid.NewGuid();
        var userBadge = GamificationTestHelpers.BuildUserBadge(userId, badgeCode: "LoginFirst", badgeName: "Primer Inicio de Sesión");

        _badgesMock
            .Setup(r => r.GetByUserAsync(userId, default))
            .ReturnsAsync(new[] { userBadge });

        var result = await _handler.HandleAsync(new GetMyBadgesQuery(userId));

        var dto = Assert.Single(result.Value);
        Assert.Equal(userBadge.Id, dto.Id);
        Assert.Equal("LoginFirst", dto.Code);
        Assert.Equal("Primer Inicio de Sesión", dto.Name);
        Assert.Null(dto.CourseId);
        Assert.Null(dto.CourseTitle);
    }

    [Fact]
    public async Task HandleAsync_CourseScopedBadge_PopulatesCourseTitle()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var userBadge = GamificationTestHelpers.BuildUserBadge(
            userId, badgeCode: "CourseDone", badgeName: "Curso Completado",
            courseId: courseId, courseTitle: "Introducción a C#");

        _badgesMock
            .Setup(r => r.GetByUserAsync(userId, default))
            .ReturnsAsync(new[] { userBadge });

        var result = await _handler.HandleAsync(new GetMyBadgesQuery(userId));

        var dto = Assert.Single(result.Value);
        Assert.Equal(courseId, dto.CourseId);
        Assert.Equal("Introducción a C#", dto.CourseTitle);
    }

    [Fact]
    public async Task HandleAsync_MultipleBadges_OrderedByObtainedAtDescending()
    {
        var userId = Guid.NewGuid();
        var older = GamificationTestHelpers.BuildUserBadge(userId, badgeCode: "LoginFirst", badgeName: "Primero");
        GamificationTestHelpers.SetPrivate(older, "ObtainedAt", DateTime.UtcNow.AddDays(-10));

        var newer = GamificationTestHelpers.BuildUserBadge(userId, badgeCode: "CourseDone", badgeName: "Último");
        GamificationTestHelpers.SetPrivate(newer, "ObtainedAt", DateTime.UtcNow.AddDays(-1));

        _badgesMock
            .Setup(r => r.GetByUserAsync(userId, default))
            .ReturnsAsync(new[] { older, newer }); // orden invertido

        var result = await _handler.HandleAsync(new GetMyBadgesQuery(userId));

        Assert.Equal("Último", result.Value[0].Name);
        Assert.Equal("Primero", result.Value[1].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(new GetMyBadgesQuery(Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }
}
