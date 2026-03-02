namespace ELearning.Application.Features.Certificates.DTOs;
public class CertificateDto { public Guid Id { get; set; } public string CourseName { get; set; } = ""; public DateTime CompletedAt { get; set; } public string CertificateUrl { get; set; } = ""; }
