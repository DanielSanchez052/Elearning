using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Certificates.Queries;

public sealed record GetCertificateByCourseQuery(Guid UserId, Guid CourseId) : IQuery<CertificateDto>;

public sealed class GetCertificateByCourseHandler : IQueryHandler<GetCertificateByCourseQuery, CertificateDto>
{
    private readonly IEnrollmentRepository _enrollments;

    public GetCertificateByCourseHandler(IEnrollmentRepository enrollments)
    {
        _enrollments = enrollments;
    }

    public async Task<Result<CertificateDto>> HandleAsync(GetCertificateByCourseQuery query, CancellationToken ct = default)
    {
        if (query.UserId == Guid.Empty)
            return Result.ValidationFailure<CertificateDto>("UserId es requerido");

        if (query.CourseId == Guid.Empty)
            return Result.ValidationFailure<CertificateDto>("CourseId es requerido");

        var enrollment = await _enrollments.GetByUserAndCourseAsync(query.UserId, query.CourseId, ct);
        if (enrollment is null)
            return Result.NotFound<CertificateDto>("No existe inscripción para este curso");

        if (!enrollment.IsCompleted || !enrollment.CompletedAt.HasValue)
            return Result.Conflict<CertificateDto>("Debes completar el curso para generar el certificado");

        return new CertificateDto(
            Id: enrollment.Id,
            CourseId: enrollment.CourseId,
            CourseName: enrollment.Course.Title,
            CompletedAt: enrollment.CompletedAt.Value,
            CertificateUrl: $"/api/certificates/courses/{enrollment.CourseId}/download"
        );
    }
}
