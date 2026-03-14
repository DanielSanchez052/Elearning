using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Users.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Users;

public class ChangeUserRoleHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly ChangeUserRoleHandler _handler;

    private static readonly Guid SuperAdminId = Guid.NewGuid();
    private static readonly Guid AdminId = Guid.NewGuid();
    private static readonly Guid TargetId = Guid.NewGuid();
    private static readonly Guid OtherTargetId = Guid.NewGuid();

    public ChangeUserRoleHandlerTests() =>
        _handler = new ChangeUserRoleHandler(_usersMock.Object);

    private void SetupTarget(User target) =>
        _usersMock
            .Setup(r => r.GetByIdTrackedAsync(target.Id, default))
            .ReturnsAsync(target);

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SuperAdminChangesStudentToInstructor_ReturnsSuccess()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.Student);
        SetupTarget(target);

        var cmd = new ChangeUserRoleCommand(TargetId, "instructor", SuperAdminId, "superadmin");
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_SuperAdminChangesRole_CallsUpdateAsync()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.Student);
        SetupTarget(target);

        await _handler.HandleAsync(new ChangeUserRoleCommand(TargetId, "instructor", SuperAdminId, "superadmin"));

        _usersMock.Verify(r => r.UpdateAsync(target, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SuperAdminChangesRole_RoleActuallyChangedOnEntity()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.Student);
        SetupTarget(target);

        await _handler.HandleAsync(new ChangeUserRoleCommand(TargetId, "instructor", SuperAdminId, "superadmin"));

        Assert.Equal(UserRole.Instructor, target.Role);
    }

    [Fact]
    public async Task HandleAsync_AdminChangesStudentToInstructor_ReturnsSuccess()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.Student);
        SetupTarget(target);

        var cmd = new ChangeUserRoleCommand(TargetId, "instructor", AdminId, "admin");
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
    }

    // ── Admin permission restrictions ──────────────────────────────────────

    [Theory]
    [InlineData("admin")]
    [InlineData("superadmin")]
    public async Task HandleAsync_AdminAssignsElevatedRole_ReturnsForbidden(string newRole)
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.Student);
        SetupTarget(target);

        var cmd = new ChangeUserRoleCommand(TargetId, newRole, AdminId, "admin");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    // ── Protección Super Admin ─────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_AdminModifiesSuperAdmin_ReturnsForbidden()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.SuperAdmin);
        SetupTarget(target);

        var cmd = new ChangeUserRoleCommand(TargetId, "student", AdminId, "admin");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_SuperAdminModifiesSuperAdmin_ReturnsSuccess()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.SuperAdmin);
        SetupTarget(target);

        var cmd = new ChangeUserRoleCommand(TargetId, "admin", SuperAdminId, "superadmin");
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
    }

    // ── Auto-cambio de rol ─────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_RequesterChangesOwnRole_ReturnsForbidden()
    {
        var target = UserHelpers.BuildUser(id: SuperAdminId, role: UserRole.SuperAdmin);
        SetupTarget(target);

        var cmd = new ChangeUserRoleCommand(SuperAdminId, "admin", SuperAdminId, "superadmin");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    // ── Rol ya asignado ────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_TargetAlreadyHasRole_ReturnsConflict()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.Instructor);
        SetupTarget(target);

        var cmd = new ChangeUserRoleCommand(TargetId, "instructor", SuperAdminId, "superadmin");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    // ── Usuario no encontrado ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_TargetNotFound_ReturnsNotFound()
    {
        _usersMock
            .Setup(r => r.GetByIdTrackedAsync(TargetId, default))
            .ReturnsAsync((User?)null);

        var cmd = new ChangeUserRoleCommand(TargetId, "instructor", SuperAdminId, "superadmin");
        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains(TargetId.ToString(), result.Error);
    }

    // ── No llama UpdateAsync cuando falla ─────────────────────────────────

    [Fact]
    public async Task HandleAsync_WhenForbidden_NeverCallsUpdateAsync()
    {
        var target = UserHelpers.BuildUser(id: TargetId, role: UserRole.Student);
        SetupTarget(target);

        // Admin intenta asignar superadmin → Forbidden antes de tocar la BD
        await _handler.HandleAsync(new ChangeUserRoleCommand(TargetId, "superadmin", AdminId, "admin"));

        _usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
