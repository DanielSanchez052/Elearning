using Microsoft.AspNetCore.Http;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

internal static class MediaHelpers
{
    public static IFormFile BuildFile(
        string fileName = "test.mp4",
        string contentType = "video/mp4",
        long sizeBytes = 1024)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(sizeBytes);
        mock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[sizeBytes > int.MaxValue ? 0 : (int)sizeBytes]));
        return mock.Object;
    }

    public static IFormFile EmptyFile(string contentType = "video/mp4")
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns("empty.file");
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(0L);
        return mock.Object;
    }

    /// <summary>Archivo que excede el límite dado en 1 byte.</summary>
    public static IFormFile Oversized(string fileName, string contentType, long maxBytes) =>
        BuildFile(fileName, contentType, maxBytes + 1);
}
