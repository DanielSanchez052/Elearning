using ELearning.API.Extensions;
using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Media.Commands;
using ELearning.Application.Features.Media.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = "instructor,admin,superadmin")]
// Solo instructores y admins pueden subir archivos —
// los estudiantes consumen el contenido pero no lo crean
public class MediaController(
    ICommandHandler<UploadVideoCommand, UploadResultDto> uploadVideoHandler,
    ICommandHandler<UploadPdfCommand, UploadResultDto> uploadPdfHandler,
    ICommandHandler<UploadThumbnailCommand, UploadResultDto> uploadThumbnailHandler
) : ControllerBase
{
    // POST api/media/videos
    // Content-Type: multipart/form-data
    [HttpPost("videos")]
    [RequestSizeLimit(524_288_000)] // 500 MB — coincide con MediaLimits.VideoMaxBytes
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        var result = await uploadVideoHandler.HandleAsync(
            new UploadVideoCommand(file),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    // POST api/media/pdfs
    // Content-Type: multipart/form-data
    [HttpPost("pdfs")]
    [RequestSizeLimit(26_214_400)] // 25 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 26_214_400)]
    public async Task<IActionResult> UploadPdf(IFormFile file)
    {
        var result = await uploadPdfHandler.HandleAsync(
            new UploadPdfCommand(file),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }

    // POST api/media/thumbnails
    // Content-Type: multipart/form-data
    [HttpPost("thumbnails")]
    [RequestSizeLimit(26_214_400)] // 25 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 26_214_400)]
    public async Task<IActionResult> UploadThumbnail(IFormFile file)
    {
        var result = await uploadThumbnailHandler.HandleAsync(
            new UploadThumbnailCommand(file),
            HttpContext.RequestAborted);
        return this.ToActionResult(result);
    }
}