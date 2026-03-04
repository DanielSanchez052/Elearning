using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Media.DTOs;
using ELearning.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace ELearning.Application.Features.Media.Commands;

public sealed record UploadPdfCommand(
    IFormFile File
) : ICommand<UploadResultDto>;

public sealed class UploadPdfHandler : ICommandHandler<UploadPdfCommand, UploadResultDto>
{
    private readonly IStorageService _storage;

    public UploadPdfHandler(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task<Result<UploadResultDto>> HandleAsync(
        UploadPdfCommand cmd,
        CancellationToken ct = default)
    {
        await using var stream = cmd.File.OpenReadStream();

        var url = await _storage.UploadAsync(
            stream: stream,
            fileName: cmd.File.FileName,
            folder: "pdfs",
            contentType: cmd.File.ContentType,
            ct: ct);

        return Result.Success(new UploadResultDto(
            Url: url,
            FileName: cmd.File.FileName,
            FileSizeBytes: cmd.File.Length,
            ContentType: cmd.File.ContentType
        ));
    }
}
