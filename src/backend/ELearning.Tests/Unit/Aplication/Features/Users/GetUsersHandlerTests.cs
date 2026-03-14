using ELearning.Application.Features.Users.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Users;

public class GetUsersHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly GetUsersHandler _handler;

    public GetUsersHandlerTests() =>
        _handler = new GetUsersHandler(_usersMock.Object);

    private void SetupPaged(IEnumerable<User> users, int total) =>
        _usersMock
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((users.ToList().AsReadOnly(), total));

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithUsers_ReturnsSuccess()
    {
        var user = UserHelpers.BuildUser();
        SetupPaged([user], 1);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_EmptyPage_ReturnsEmptyItems()
    {
        SetupPaged([], 0);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    // ── Mapeo DTO ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserMappedToDto_AllFieldsCorrect()
    {
        var userId = Guid.NewGuid();
        var user = UserHelpers.BuildUser(
            id: userId,
            fullName: "Ana García",
            email: "ana@example.com",
            role: UserRole.Instructor,
            countryId: 3,
            countryName: "México",
            verified: true);
        SetupPaged([user], 1);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null));

        var dto = result.Value.Items[0];
        Assert.Equal(userId, dto.Id);
        Assert.Equal("Ana García", dto.FullName);
        Assert.Equal("ana@example.com", dto.Email);
        Assert.Equal("instructor", dto.Role);   // .ToLowerInvariant()
        Assert.Equal("México", dto.Country);
        Assert.Equal(3, dto.CountryId);
        Assert.True(dto.IsEmailVerified);
    }

    [Fact]
    public async Task HandleAsync_RoleInDto_IsAlwaysLowercase()
    {
        var user = UserHelpers.BuildUser(role: UserRole.SuperAdmin);
        SetupPaged([user], 1);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null));

        Assert.Equal("superadmin", result.Value.Items[0].Role);
    }

    // ── Paginación ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_TotalPages_CalculatedCorrectly()
    {
        // 25 items con pageSize=10 → 3 páginas
        SetupPaged(Enumerable.Range(0, 10).Select(_ => UserHelpers.BuildUser()), 25);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null, Page: 1, PageSize: 10));

        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(25, result.Value.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_ExactMultipleOfPageSize_TotalPagesExact()
    {
        // 20 items con pageSize=10 → 2 páginas (sin redondeo extra)
        SetupPaged(Enumerable.Range(0, 10).Select(_ => UserHelpers.BuildUser()), 20);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null, Page: 1, PageSize: 10));

        Assert.Equal(2, result.Value.TotalPages);
    }

    [Fact]
    public async Task HandleAsync_PageBelowOne_ClampedToOne()
    {
        SetupPaged([], 0);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null, Page: -5));

        Assert.Equal(1, result.Value.Page);
    }

    [Fact]
    public async Task HandleAsync_PageSizeAbove100_ClampedTo100()
    {
        SetupPaged([], 0);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null, PageSize: 500));

        Assert.Equal(100, result.Value.PageSize);
    }

    [Fact]
    public async Task HandleAsync_PageSizeBelow1_ClampedTo1()
    {
        SetupPaged([], 0);

        var result = await _handler.HandleAsync(new GetUsersQuery(null, null, null, null, PageSize: 0));

        Assert.Equal(1, result.Value.PageSize);
    }

    // ── Filtros pasados al repositorio ────────────────────────────────────

    [Fact]
    public async Task HandleAsync_FiltersPassedThrough_ToRepository()
    {
        SetupPaged([], 0);

        await _handler.HandleAsync(new GetUsersQuery(
            CountryId: 5,
            Role: "instructor",
            Search: "carlos",
            IsEmailVerified: true,
            Page: 2,
            PageSize: 15));

        _usersMock.Verify(r => r.GetPagedAsync(
            5, "instructor", "carlos", true, 2, 15, default), Times.Once);
    }
}
