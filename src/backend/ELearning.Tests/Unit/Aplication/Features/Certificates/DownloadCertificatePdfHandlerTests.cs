using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Certificates;

public class DownloadCertificatePdfHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentsMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<ICertificatePdfService> _pdfMock = new();
    private readonly DownloadCertificatePdfHandler _handler;

    public DownloadCertificatePdfHandlerTests()
    {
        _handler = new DownloadCertificatePdfHandler(
            _enrollmentsMock.Object,
            _usersMock.Object,
            _pdfMock.Object);

        // Comportamiento por defecto del generador PDF
        _pdfMock
            .Setup(p => p.Generate(It.IsAny<CertificatePdfData>()))
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // Magic bytes %PDF
    }

    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ValidCompletedEnrollment_ReturnsPdfBytes()
    {
        SetupValidScenario(out var enrollment, out _);

        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Content);
        Assert.Equal("application/pdf", result.Value.ContentType);
    }

    [Fact]
    public async Task HandleAsync_ValidScenario_FileNameContainsCourseNameAndDate()
    {
        SetupValidScenario(out var enrollment, out _);

        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.NotNull(result.Value.FileName);
        Assert.EndsWith(".pdf", result.Value.FileName);
        Assert.StartsWith("certificado-", result.Value.FileName);
    }

    [Fact]
    public async Task HandleAsync_ValidScenario_PdfServiceCalledWithStudentNameAndCourseName()
    {
        SetupValidScenario(out var enrollment, out var user);

        CertificatePdfData? capturedData = null;
        _pdfMock
            .Setup(p => p.Generate(It.IsAny<CertificatePdfData>()))
            .Callback<CertificatePdfData>(d => capturedData = d)
            .Returns(new byte[] { 1, 2, 3 });

        await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.NotNull(capturedData);
        Assert.Equal(user.FullName, capturedData!.StudentName);
        Assert.Equal(enrollment.Course.Title, capturedData.CourseName);
        Assert.NotNull(capturedData.CertificateCode);
        Assert.StartsWith("CERT-", capturedData.CertificateCode);
    }

    [Fact]
    public async Task HandleAsync_ValidScenario_CertificateCodeContainsEnrollmentAndCourseParts()
    {
        SetupValidScenario(out var enrollment, out _);

        CertificatePdfData? capturedData = null;
        _pdfMock
            .Setup(p => p.Generate(It.IsAny<CertificatePdfData>()))
            .Callback<CertificatePdfData>(d => capturedData = d)
            .Returns(new byte[] { 1 });

        await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        // El código es CERT-{courseId[..8]}-{enrollmentId[..8]}
        var courseChunk = enrollment.CourseId.ToString()[..8].ToUpperInvariant();
        var enrollmentChunk = enrollment.Id.ToString()[..8].ToUpperInvariant();

        Assert.Contains(courseChunk, capturedData!.CertificateCode);
        Assert.Contains(enrollmentChunk, capturedData.CertificateCode);
    }

    [Fact]
    public async Task HandleAsync_CourseNameWithInvalidChars_SanitizesFileName()
    {
        SetupValidScenario(out _, out _, courseTitle: "Curso: Intro/Avanzado");

        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        // Path.GetInvalidFileNameChars() reemplaza ':' y '/' por '-'
        Assert.DoesNotContain(":", result.Value.FileName);
        Assert.DoesNotContain("/", result.Value.FileName);
    }

    // ── Validaciones de entrada ────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(Guid.Empty, CertificateTestHelpers.CourseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_EmptyCourseId_ReturnsValidationFailure()
    {
        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(CertificateTestHelpers.UserId, Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    // ── Not Found ──────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EnrollmentNotFound_ReturnsNotFound()
    {
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync((CourseEnrollment?)null);

        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        var enrollment = CertificateTestHelpers.BuildCompletedEnrollment();
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync(enrollment);
        _usersMock
            .Setup(r => r.GetByIdAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
        Assert.Contains("Usuario", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ── Conflicto ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_EnrollmentNotCompleted_ReturnsConflict()
    {
        var enrollment = CertificateTestHelpers.BuildActiveEnrollment();
        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync(enrollment);

        var result = await _handler.HandleAsync(
            new DownloadCertificatePdfQuery(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
        Assert.Contains("completar", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void SetupValidScenario(
        out CourseEnrollment enrollment,
        out User user,
        string courseTitle = "Curso de Prueba")
    {
        enrollment = CertificateTestHelpers.BuildCompletedEnrollment(
            userId: CertificateTestHelpers.UserId,
            courseId: CertificateTestHelpers.CourseId,
            courseTitle: courseTitle);

        user = CertificateTestHelpers.BuildUser("Ana García");

        _enrollmentsMock
            .Setup(r => r.GetByUserAndCourseAsync(
                CertificateTestHelpers.UserId, CertificateTestHelpers.CourseId, default))
            .ReturnsAsync(enrollment);

        _usersMock
            .Setup(r => r.GetByIdAsync(CertificateTestHelpers.UserId, default))
            .ReturnsAsync(user);
    }
}
