using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.DTOs;

namespace ELearning.Application.Features.Certificates.Queries;

public class GetMyCertificatesQuery : IQuery<List<CertificateDto>> { }
public class GetMyCertificatesHandler : IQueryHandler<GetMyCertificatesQuery, List<CertificateDto>> {
    Task<Result<List<CertificateDto>>> IQueryHandler<GetMyCertificatesQuery, List<CertificateDto>>.HandleAsync(GetMyCertificatesQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class GetCertificateByCourseQuery : IQuery<CertificateDto> { public Guid CourseId { get; set; } }
public class GetCertificateByCourseHandler : IQueryHandler<GetCertificateByCourseQuery, CertificateDto> { 
    Task<Result<CertificateDto>> IQueryHandler<GetCertificateByCourseQuery, CertificateDto>.HandleAsync(GetCertificateByCourseQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
