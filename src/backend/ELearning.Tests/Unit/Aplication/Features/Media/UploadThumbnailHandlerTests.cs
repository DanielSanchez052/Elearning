using ELearning.Application.Features.Media.Commands;
using ELearning.Domain.Interfaces.Services;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class UploadThumbnailHandlerTests
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly UploadThumbnailHandler _handler;

    private const string FakeUrl = "http://localhost:5000/uploads/thumbnails/abc_cover.jpg";

    public UploadThumbnailHandlerTests()
    {
        _handler = new UploadThumbnailHandler(_storageMock.Object);

        _storageMock
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeUrl);
    }

    [Fact]
    public async Task HandleAsync_ValidJpeg_ReturnsSuccess()
    {
        var file = MediaHelpers.BuildFile("cover.jpg", "image/jpeg", 512 * 1024);
        var result = await _handler.HandleAsync(new UploadThumbnailCommand(file));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidPng_ReturnsSuccess()
    {
        var file = MediaHelpers.BuildFile("cover.png", "image/png", 512 * 1024);
        var result = await _handler.HandleAsync(new UploadThumbnailCommand(file));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidWebp_ReturnsSuccess()
    {
        var file = MediaHelpers.BuildFile("cover.webp", "image/webp", 512 * 1024);
        var result = await _handler.HandleAsync(new UploadThumbnailCommand(file));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidThumbnail_UrlComesFromStorage()
    {
        var file = MediaHelpers.BuildFile("cover.jpg", "image/jpeg", 512 * 1024);
        var result = await _handler.HandleAsync(new UploadThumbnailCommand(file));

        Assert.Equal(FakeUrl, result.Value.Url);
    }

    [Fact]
    public async Task HandleAsync_ValidThumbnail_DtoContainsFileMetadata()
    {
        var file = MediaHelpers.BuildFile("cover.jpg", "image/jpeg", 800 * 1024);
        var result = await _handler.HandleAsync(new UploadThumbnailCommand(file));

        Assert.Equal("cover.jpg", result.Value.FileName);
        Assert.Equal("image/jpeg", result.Value.ContentType);
        Assert.Equal(800 * 1024, result.Value.FileSizeBytes);
    }

    [Fact]
    public async Task HandleAsync_ValidThumbnail_UploadsToThumbnailsFolder()
    {
        var file = MediaHelpers.BuildFile("cover.jpg", "image/jpeg", 1024);
        await _handler.HandleAsync(new UploadThumbnailCommand(file));

        _storageMock.Verify(s => s.UploadAsync(
            It.IsAny<Stream>(),
            "cover.jpg",
            "thumbnails",    // carpeta exacta definida en el handler
            "image/jpeg",
            default), Times.Once);
    }
}
