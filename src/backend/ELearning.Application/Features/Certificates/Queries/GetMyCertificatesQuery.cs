using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Certificates.Queries;

public sealed record GetMyCertificatesQuery(Guid UserId) : IQuery<List<CertificateDto>>;

public sealed class GetMyCertificatesHandler : IQueryHandler<GetMyCertificatesQuery, List<CertificateDto>>
{
    private readonly IEnrollmentRepository _enrollments;

    public GetMyCertificatesHandler(IEnrollmentRepository enrollments)
    {
        _enrollments = enrollments;
    }

    public async Task<Result<List<CertificateDto>>> HandleAsync(GetMyCertificatesQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty)
            return Result.ValidationFailure<List<CertificateDto>>("UserId es requerido");

        var enrollments = await _enrollments.GetByUserAsync(query.UserId, ct);

        var certificates = enrollments
            .Where(e => e.IsCompleted && e.CompletedAt.HasValue)
            .OrderByDescending(e => e.CompletedAt)
            .Select(e => new CertificateDto(
                Id: e.Id,
                CourseId: e.CourseId,
                CourseName: e.Course.Title,
                CompletedAt: e.CompletedAt!.Value,
                CertificateUrl: $"/api/certificates/courses/{e.CourseId}/download"
            ))
            .ToList();

        return certificates;
    }
}
