using ELearning.Application.Features.Media.constants;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class MediaLimitsContractTests
{
    [Fact]
    public void VideoMaxBytes_Is500MB() =>
        Assert.Equal(500L * 1024 * 1024, MediaLimits.VideoMaxBytes);

    [Fact]
    public void DocumentMaxBytes_Is25MB() =>
        Assert.Equal(25L * 1024 * 1024, MediaLimits.DocumentMaxBytes);

    [Fact]
    public void ThumbnailMaxBytes_Is25MB() =>
        Assert.Equal(25L * 1024 * 1024, MediaLimits.ThumbnailMaxBytes);

    [Fact]
    public void VideoContentTypes_ContainsOnlyMp4() =>
        Assert.Equal(new[] { "video/mp4" }, MediaLimits.VideoContentTypes);

    [Fact]
    public void DocumentContentTypes_ContainsOnlyPdf() =>
        Assert.Equal(new[] { "application/pdf" }, MediaLimits.DocumentContentTypes);

    [Fact]
    public void ThumbnailContentTypes_ContainsJpegPngWebp()
    {
        Assert.Contains("image/jpeg", MediaLimits.ThumbnailContentTypes);
        Assert.Contains("image/png", MediaLimits.ThumbnailContentTypes);
        Assert.Contains("image/webp", MediaLimits.ThumbnailContentTypes);
    }

    [Fact]
    public void ThumbnailContentTypes_HasExactly3Types() =>
        Assert.Equal(3, MediaLimits.ThumbnailContentTypes.Length);
}
