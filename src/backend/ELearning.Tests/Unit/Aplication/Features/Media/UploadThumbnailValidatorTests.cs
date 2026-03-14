using ELearning.Application.Features.Media.Commands;
using ELearning.Application.Features.Media.constants;
using ELearning.Application.Features.Media.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class UploadThumbnailValidatorTests
{
    private readonly UploadThumbnailValidator _validator = new();

    // ── Archivo vacío ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyFile_HasFileError()
    {
        var file = MediaHelpers.EmptyFile("image/jpeg");
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadThumbnailCommand.File));
    }

    [Fact]
    public void Validate_EmptyFile_EarlyReturn_ExactlyOneError()
    {
        var file = MediaHelpers.EmptyFile("image/jpeg");
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        Assert.Single(result.Errors);
    }

    // ── Content types aceptados ────────────────────────────────────────────

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Validate_AllowedContentType_IsValid(string contentType)
    {
        var file = MediaHelpers.BuildFile("cover.img", contentType, 512 * 1024);
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("IMAGE/JPEG")]
    [InlineData("Image/Png")]
    [InlineData("IMAGE/WEBP")]
    public void Validate_AllowedContentTypeUpperCase_IsValid(string contentType)
    {
        // .ToLowerInvariant() antes de Contains → case-insensitive
        var file = MediaHelpers.BuildFile("cover.img", contentType, 512 * 1024);
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        Assert.True(result.IsValid);
    }

    // ── Content types rechazados ───────────────────────────────────────────

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/bmp")]
    [InlineData("image/tiff")]
    [InlineData("image/svg+xml")]
    [InlineData("video/mp4")]
    [InlineData("application/pdf")]
    public void Validate_RejectedContentType_HasFileError(string contentType)
    {
        var file = MediaHelpers.BuildFile("cover.gif", contentType, 512 * 1024);
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadThumbnailCommand.File));
    }

    // ── Tamaño ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ExceedsThumbnailMaxBytes_HasFileError()
    {
        var file = MediaHelpers.Oversized("big.jpg", "image/jpeg", MediaLimits.ThumbnailMaxBytes);
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadThumbnailCommand.File));
    }

    [Fact]
    public void Validate_ExactlyThumbnailMaxBytes_IsValid()
    {
        var file = MediaHelpers.BuildFile("big.jpg", "image/jpeg", MediaLimits.ThumbnailMaxBytes);
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        Assert.True(result.IsValid);
    }

    // ── Mensaje de error ───────────────────────────────────────────────────

    [Fact]
    public void Validate_InvalidContentType_ErrorMessageMentionsJpegPngWebp()
    {
        var file = MediaHelpers.BuildFile("cover.gif", "image/gif", 512 * 1024);
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        // Mensaje real: "Solo se permiten imágenes JPEG, PNG o WebP."
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UploadThumbnailCommand.File) &&
            (e.ErrorMessage.Contains("JPEG", StringComparison.OrdinalIgnoreCase) ||
             e.ErrorMessage.Contains("PNG", StringComparison.OrdinalIgnoreCase) ||
             e.ErrorMessage.Contains("WebP", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Validate_ExceedsSize_ErrorMessageMentionsMegabytes()
    {
        var file = MediaHelpers.Oversized("big.jpg", "image/jpeg", MediaLimits.ThumbnailMaxBytes);
        var result = _validator.Validate(new UploadThumbnailCommand(file));

        // Mensaje real: "La imagen no puede superar 25 MB."
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UploadThumbnailCommand.File) &&
            e.ErrorMessage.Contains("25"));
    }
}
