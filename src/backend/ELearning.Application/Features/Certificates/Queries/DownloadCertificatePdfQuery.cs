using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.DTOs;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Domain.Interfaces.Services;

namespace ELearning.Application.Features.Certificates.Queries;

public sealed record DownloadCertificatePdfQuery(Guid UserId, Guid CourseId) : IQuery<CertificateFileDto>;

public sealed class DownloadCertificatePdfHandler : IQueryHandler<DownloadCertificatePdfQuery, CertificateFileDto>
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IUserRepository _users;
    private readonly ICertificatePdfService _certificatePdfService;

    public DownloadCertificatePdfHandler(
        IEnrollmentRepository enrollments,
        IUserRepository users,
        ICertificatePdfService certificatePdfService)
    {
        _enrollments = enrollments;
        _users = users;
        _certificatePdfService = certificatePdfService;
    }

    public async Task<Result<CertificateFileDto>> HandleAsync(DownloadCertificatePdfQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty)
            return Result.ValidationFailure<CertificateFileDto>("UserId es requerido");

        if (query.CourseId == Guid.Empty)
            return Result.ValidationFailure<CertificateFileDto>("CourseId es requerido");

        var enrollment = await _enrollments.GetByUserAndCourseAsync(query.UserId, query.CourseId, ct);
        if (enrollment is null)
            return Result.NotFound<CertificateFileDto>("No existe inscripción para este curso");

        if (!enrollment.IsCompleted || !enrollment.CompletedAt.HasValue)
            return Result.Conflict<CertificateFileDto>("Debes completar el curso para descargar el certificado");

        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Result.NotFound<CertificateFileDto>("Usuario no encontrado");

        var certificateCode = $"CERT-{enrollment.CourseId.ToString()[..8].ToUpperInvariant()}-{enrollment.Id.ToString()[..8].ToUpperInvariant()}";

        var pdfBytes = _certificatePdfService.Generate(new CertificatePdfData(
            StudentName: user.FullName,
            CourseName: enrollment.Course.Title,
            CompletedAt: enrollment.CompletedAt.Value,
            CertificateCode: certificateCode
        ));

        var safeCourseName = SanitizeFileName(enrollment.Course.Title);
        var fileName = $"certificado-{safeCourseName}-{enrollment.CompletedAt.Value:yyyyMMdd}.pdf";

        return new CertificateFileDto(
            Content: pdfBytes,
            ContentType: "application/pdf",
            FileName: fileName
        );
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value
            .Select(ch => invalid.Contains(ch) ? '-' : ch)
            .ToArray())
            .Trim();

        return string.IsNullOrWhiteSpace(sanitized) ? "curso" : sanitized;
    }
}
