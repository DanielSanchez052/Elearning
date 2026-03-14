using ELearning.Application.Features.Media.Commands;
using ELearning.Application.Features.Media.constants;
using ELearning.Application.Features.Media.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class UploadVideoValidatorTests
{
    private readonly UploadVideoValidator _validator = new();

    // ── Archivo vacío ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyFile_HasFileError()
    {
        var file = MediaHelpers.EmptyFile("video/mp4");
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadVideoCommand.File));
    }

    [Fact]
    public void Validate_EmptyFile_EarlyReturn_ExactlyOneError()
    {
        // El validator hace early return cuando Length == 0
        var file = MediaHelpers.EmptyFile("video/mp4");
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.Single(result.Errors);
    }

    // ── Content type ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("video/avi")]
    [InlineData("video/quicktime")]
    [InlineData("video/webm")]
    [InlineData("application/octet-stream")]
    [InlineData("image/jpeg")]
    [InlineData("application/pdf")]
    public void Validate_InvalidContentType_HasFileError(string contentType)
    {
        var file = MediaHelpers.BuildFile("video.mp4", contentType, 1024);
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadVideoCommand.File));
    }

    [Fact]
    public void Validate_VideoMp4LowerCase_IsValid()
    {
        var file = MediaHelpers.BuildFile("lecture.mp4", "video/mp4", 10 * 1024 * 1024);
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_VideoMp4UpperCase_IsValid()
    {
        // El validator normaliza con .ToLowerInvariant() antes de comparar
        var file = MediaHelpers.BuildFile("lecture.mp4", "VIDEO/MP4", 10 * 1024 * 1024);
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.True(result.IsValid);
    }

    // ── Tamaño ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ExceedsVideoMaxBytes_HasFileError()
    {
        var file = MediaHelpers.Oversized("big.mp4", "video/mp4", MediaLimits.VideoMaxBytes);
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadVideoCommand.File));
    }

    [Fact]
    public void Validate_ExactlyVideoMaxBytes_IsValid()
    {
        var file = MediaHelpers.BuildFile("big.mp4", "video/mp4", MediaLimits.VideoMaxBytes);
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_SmallFile_IsValid()
    {
        var file = MediaHelpers.BuildFile("small.mp4", "video/mp4", 1024);
        var result = _validator.Validate(new UploadVideoCommand(file));

        Assert.True(result.IsValid);
    }

    // ── Mensaje de error ───────────────────────────────────────────────────

    [Fact]
    public void Validate_InvalidContentType_ErrorMessageMentionsMp4()
    {
        var file = MediaHelpers.BuildFile("video.avi", "video/avi", 1024);
        var result = _validator.Validate(new UploadVideoCommand(file));

        // Mensaje real: "Solo se permiten archivos MP4."
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UploadVideoCommand.File) &&
            e.ErrorMessage.Contains("MP4", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ExceedsSize_ErrorMessageMentionsMegabytes()
    {
        var file = MediaHelpers.Oversized("big.mp4", "video/mp4", MediaLimits.VideoMaxBytes);
        var result = _validator.Validate(new UploadVideoCommand(file));

        // Mensaje real: "El video no puede superar 500 MB."
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UploadVideoCommand.File) &&
            e.ErrorMessage.Contains("500"));
    }
}
