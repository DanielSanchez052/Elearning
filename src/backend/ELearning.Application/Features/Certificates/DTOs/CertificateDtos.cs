namespace ELearning.Application.Features.Certificates.DTOs;

public sealed record CertificateDto(
    Guid Id,
    Guid CourseId,
    string CourseName,
    DateTime CompletedAt,
    string CertificateUrl
);

public sealed record CertificateFileDto(
    byte[] Content,
    string ContentType,
    string FileName
);
