using ELearning.Application.Features.Media.Commands;
using ELearning.Application.Features.Media.constants;
using ELearning.Application.Features.Media.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class UploadPdfValidatorTests
{
    private readonly UploadPdfValidator _validator = new();

    // ── Archivo vacío ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyFile_HasFileError()
    {
        var file = MediaHelpers.EmptyFile("application/pdf");
        var result = _validator.Validate(new UploadPdfCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadPdfCommand.File));
    }

    [Fact]
    public void Validate_EmptyFile_EarlyReturn_ExactlyOneError()
    {
        var file = MediaHelpers.EmptyFile("application/pdf");
        var result = _validator.Validate(new UploadPdfCommand(file));

        Assert.Single(result.Errors);
    }

    // ── Content type ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("application/msword")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("text/plain")]
    [InlineData("image/png")]
    [InlineData("video/mp4")]
    public void Validate_InvalidContentType_HasFileError(string contentType)
    {
        var file = MediaHelpers.BuildFile("doc.pdf", contentType, 1024);
        var result = _validator.Validate(new UploadPdfCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadPdfCommand.File));
    }

    [Fact]
    public void Validate_ApplicationPdf_IsValid()
    {
        var file = MediaHelpers.BuildFile("manual.pdf", "application/pdf", 1 * 1024 * 1024);
        var result = _validator.Validate(new UploadPdfCommand(file));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ApplicationPdfUpperCase_IsValid()
    {
        // .ToLowerInvariant() → case-insensitive
        var file = MediaHelpers.BuildFile("manual.pdf", "APPLICATION/PDF", 1 * 1024 * 1024);
        var result = _validator.Validate(new UploadPdfCommand(file));

        Assert.True(result.IsValid);
    }

    // ── Tamaño ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ExceedsDocumentMaxBytes_HasFileError()
    {
        var file = MediaHelpers.Oversized("big.pdf", "application/pdf", MediaLimits.DocumentMaxBytes);
        var result = _validator.Validate(new UploadPdfCommand(file));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UploadPdfCommand.File));
    }

    [Fact]
    public void Validate_ExactlyDocumentMaxBytes_IsValid()
    {
        var file = MediaHelpers.BuildFile("big.pdf", "application/pdf", MediaLimits.DocumentMaxBytes);
        var result = _validator.Validate(new UploadPdfCommand(file));

        Assert.True(result.IsValid);
    }

    // ── Mensaje de error ───────────────────────────────────────────────────

    [Fact]
    public void Validate_InvalidContentType_ErrorMessageMentionsPdf()
    {
        var file = MediaHelpers.BuildFile("doc.docx", "application/msword", 1024);
        var result = _validator.Validate(new UploadPdfCommand(file));

        // Mensaje real: "Solo se permiten archivos PDF."
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UploadPdfCommand.File) &&
            e.ErrorMessage.Contains("PDF", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ExceedsSize_ErrorMessageMentionsMegabytes()
    {
        var file = MediaHelpers.Oversized("big.pdf", "application/pdf", MediaLimits.DocumentMaxBytes);
        var result = _validator.Validate(new UploadPdfCommand(file));

        // Mensaje real: "El PDF no puede superar 25 MB."
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UploadPdfCommand.File) &&
            e.ErrorMessage.Contains("25"));
    }
}
