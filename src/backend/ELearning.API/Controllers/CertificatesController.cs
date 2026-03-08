using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Certificates.DTOs;
using ELearning.Application.Features.Certificates.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
 [Authorize]
public class CertificatesController(
    IQueryHandler<GetMyCertificatesQuery, List<CertificateDto>> getMyCertificatesHandler,
    IQueryHandler<GetCertificateByCourseQuery, CertificateDto> getCertificateByCourseHandler,
    IQueryHandler<DownloadCertificatePdfQuery, CertificateFileDto> downloadCertificatePdfHandler
) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyCertificates(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await getMyCertificatesHandler.HandleAsync(new GetMyCertificatesQuery(userId), ct);
        return this.ToActionResult(result);
    }

    [HttpGet("courses/{courseId:guid}")]
    public async Task<IActionResult> GetCertificateByCourse(Guid courseId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await getCertificateByCourseHandler.HandleAsync(new GetCertificateByCourseQuery(userId, courseId), ct);
        return this.ToActionResult(result);
    }

    [HttpGet("courses/{courseId:guid}/download")]
    public async Task<IActionResult> DownloadByCourse(Guid courseId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await downloadCertificatePdfHandler.HandleAsync(new DownloadCertificatePdfQuery(userId, courseId), ct);

        if (result.IsFailure)
            return this.ToActionResult(result);

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }
}
