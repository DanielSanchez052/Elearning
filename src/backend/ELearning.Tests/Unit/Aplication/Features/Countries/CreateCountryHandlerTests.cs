using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Commands;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Countries;

public class CreateCountryHandlerTests
{
    private readonly Mock<ICountryRepository> _countriesMock = new();
    private readonly CreateCountryHandler _handler;

    public CreateCountryHandlerTests() =>
        _handler = new CreateCountryHandler(_countriesMock.Object);

    private static CreateCountryCommand ValidCmd() => new("COL", "Colombia");

    [Fact]
    public async Task HandleAsync_NewCode_ReturnsSuccessWithId()
    {
        _countriesMock
            .Setup(r => r.ExistsByCodeAsync("col", default))
            .ReturnsAsync(false);
        _countriesMock
            .Setup(r => r.CreateAsync(It.IsAny<Country>(), default))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(ValidCmd());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_DuplicateCode_ReturnsConflict()
    {
        _countriesMock
            .Setup(r => r.ExistsByCodeAsync("col", default))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(ValidCmd());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
        Assert.Contains("COL", result.Error);
    }

    [Fact]
    public async Task HandleAsync_NewCountry_CodeStoredUppercase()
    {
        Country? created = null;
        _countriesMock
            .Setup(r => r.ExistsByCodeAsync("col", default))
            .ReturnsAsync(false);
        _countriesMock
            .Setup(r => r.CreateAsync(It.IsAny<Country>(), default))
            .Callback<Country, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(new CreateCountryCommand("col", "Colombia"));

        Assert.NotNull(created);
        Assert.Equal("COL", created!.Code);
    }

    [Fact]
    public async Task HandleAsync_NewCountry_NameTrimmed()
    {
        Country? created = null;
        _countriesMock
            .Setup(r => r.ExistsByCodeAsync("col", default))
            .ReturnsAsync(false);
        _countriesMock
            .Setup(r => r.CreateAsync(It.IsAny<Country>(), default))
            .Callback<Country, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(new CreateCountryCommand("COL", "  Colombia  "));

        Assert.Equal("Colombia", created!.Name);
    }

    [Fact]
    public async Task HandleAsync_CodeCheckIsLowercase()
    {
        // El handler llama ExistsByCodeAsync con code.ToLower()
        _countriesMock
            .Setup(r => r.ExistsByCodeAsync("mex", default))
            .ReturnsAsync(false);
        _countriesMock
            .Setup(r => r.CreateAsync(It.IsAny<Country>(), default))
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(new CreateCountryCommand("MEX", "México"));

        _countriesMock.Verify(r => r.ExistsByCodeAsync("mex", default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Success_CallsCreateAsync()
    {
        _countriesMock
            .Setup(r => r.ExistsByCodeAsync("col", default))
            .ReturnsAsync(false);

        await _handler.HandleAsync(ValidCmd());

        _countriesMock.Verify(r => r.CreateAsync(It.IsAny<Country>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NewCountry_IsActiveByDefault()
    {
        Country? created = null;
        _countriesMock
            .Setup(r => r.ExistsByCodeAsync("col", default))
            .ReturnsAsync(false);
        _countriesMock
            .Setup(r => r.CreateAsync(It.IsAny<Country>(), default))
            .Callback<Country, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(ValidCmd());

        Assert.True(created!.IsActive);
    }
}
