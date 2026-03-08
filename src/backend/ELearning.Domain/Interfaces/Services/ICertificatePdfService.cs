namespace ELearning.Domain.Interfaces.Services;

public sealed record CertificatePdfData(
    string StudentName,
    string CourseName,
    DateTime CompletedAt,
    string CertificateCode
);

public interface ICertificatePdfService
{
    byte[] Generate(CertificatePdfData data);
}
