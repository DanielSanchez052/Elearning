using ELearning.Application.Features.Media.Commands;
using ELearning.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class UploadPdfHandlerTests
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly UploadPdfHandler _handler;

    private const string FakeUrl = "http://localhost:5000/uploads/pdfs/abc_manual.pdf";

    public UploadPdfHandlerTests()
    {
        _handler = new UploadPdfHandler(_storageMock.Object);

        _storageMock
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeUrl);
    }

    [Fact]
    public async Task HandleAsync_ValidPdf_ReturnsSuccess()
    {
        var file = MediaHelpers.BuildFile("manual.pdf", "application/pdf", 2 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadPdfCommand(file));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_ValidPdf_UrlComesFromStorage()
    {
        var file = MediaHelpers.BuildFile("manual.pdf", "application/pdf", 2 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadPdfCommand(file));

        Assert.Equal(FakeUrl, result.Value.Url);
    }

    [Fact]
    public async Task HandleAsync_ValidPdf_DtoContainsOriginalFileName()
    {
        var file = MediaHelpers.BuildFile("manual.pdf", "application/pdf", 3 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadPdfCommand(file));

        Assert.Equal("manual.pdf", result.Value.FileName);
    }

    [Fact]
    public async Task HandleAsync_ValidPdf_DtoContainsFileSizeAndContentType()
    {
        var file = MediaHelpers.BuildFile("manual.pdf", "application/pdf", 3 * 1024 * 1024);
        var result = await _handler.HandleAsync(new UploadPdfCommand(file));

        Assert.Equal(3 * 1024 * 1024, result.Value.FileSizeBytes);
        Assert.Equal("application/pdf", result.Value.ContentType);
    }

    [Fact]
    public async Task HandleAsync_ValidPdf_UploadsToPdfsFolder()
    {
        var file = MediaHelpers.BuildFile("manual.pdf", "application/pdf", 1024);
        await _handler.HandleAsync(new UploadPdfCommand(file));

        _storageMock.Verify(s => s.UploadAsync(
            It.IsAny<Stream>(),
            "manual.pdf",
            "pdfs",             // carpeta exacta definida en el handler
            "application/pdf",
            default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidPdf_CallsOpenReadStream()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("manual.pdf");
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.Length).Returns(1024L);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

        await _handler.HandleAsync(new UploadPdfCommand(fileMock.Object));

        fileMock.Verify(f => f.OpenReadStream(), Times.Once);
    }
}
