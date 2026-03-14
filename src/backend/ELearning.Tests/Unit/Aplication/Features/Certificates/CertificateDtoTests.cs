using ELearning.Application.Features.Certificates.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Tests.Unit.Aplication.Features.Certificates;

public class CertificateDtoTests
{
    [Fact]
    public void CertificateDto_CanBeCreated_WithAllFields()
    {
        var id = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var completedAt = DateTime.UtcNow;
        var url = $"/api/certificates/courses/{courseId}/download";

        var dto = new CertificateDto(
            Id: id,
            CourseId: courseId,
            CourseName: "Mi Curso",
            CompletedAt: completedAt,
            CertificateUrl: url);

        Assert.Equal(id, dto.Id);
        Assert.Equal(courseId, dto.CourseId);
        Assert.Equal("Mi Curso", dto.CourseName);
        Assert.Equal(completedAt, dto.CompletedAt);
        Assert.Equal(url, dto.CertificateUrl);
    }

    [Fact]
    public void CertificateFileDto_ContentTypeShouldBePdf()
    {
        var dto = new CertificateFileDto(
            Content: new byte[] { 0x25, 0x50, 0x44, 0x46 },
            ContentType: "application/pdf",
            FileName: "certificado-test.pdf");

        Assert.Equal("application/pdf", dto.ContentType);
        Assert.EndsWith(".pdf", dto.FileName);
        Assert.NotEmpty(dto.Content);
    }
}
