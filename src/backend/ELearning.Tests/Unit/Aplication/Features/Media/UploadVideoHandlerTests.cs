using ELearning.Application.Features.Media.Commands;
using ELearning.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class UploadVideoHandlerTests
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly UploadVideoHandler _handler;

    private const string FakeUrl = "http://localhost:5000/uploads/videos/abc_lecture.mp4";

    public UploadVideoHandlerTests()
    {
        _handler = new UploadVideoHandler(_storageMock.Object);

        _storageMock
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeUrl);
    }

    [Fact]
    public async Task HandleAsync_ValidVideo_ReturnsSuccess()
    {
        var file = MediaHelpers.BuildFile("lecture.mp4", "video/mp4", 10 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadVideoCommand(file));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidVideo_UrlComesFromStorage()
    {
        var file = MediaHelpers.BuildFile("lecture.mp4", "video/mp4", 10 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadVideoCommand(file));

        Assert.Equal(FakeUrl, result.Value.Url);
    }

    [Fact]
    public async Task HandleAsync_ValidVideo_DtoContainsOriginalFileName()
    {
        var file = MediaHelpers.BuildFile("lecture.mp4", "video/mp4", 5 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadVideoCommand(file));

        Assert.Equal("lecture.mp4", result.Value.FileName);
    }

    [Fact]
    public async Task HandleAsync_ValidVideo_DtoContainsFileSizeAndContentType()
    {
        var file = MediaHelpers.BuildFile("lecture.mp4", "video/mp4", 5 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadVideoCommand(file));

        Assert.Equal(5 * 1024 * 1024, result.Value.FileSizeBytes);
        Assert.Equal("video/mp4", result.Value.ContentType);
    }

    [Fact]
    public async Task HandleAsync_ValidVideo_UploadsToVideosFolder()
    {
        var file = MediaHelpers.BuildFile("lecture.mp4", "video/mp4", 1024);
        await _handler.HandleAsync(new UploadVideoCommand(file));

        _storageMock.Verify(s => s.UploadAsync(
            It.IsAny<Stream>(),
            "lecture.mp4",
            "videos",       // carpeta exacta definida en el handler
            "video/mp4",
            default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidVideo_CallsOpenReadStream()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("lecture.mp4");
        fileMock.Setup(f => f.ContentType).Returns("video/mp4");
        fileMock.Setup(f => f.Length).Returns(1024L);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

        await _handler.HandleAsync(new UploadVideoCommand(fileMock.Object));

        fileMock.Verify(f => f.OpenReadStream(), Times.Once);
    }
}
