using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Media.DTOs;
using ELearning.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace ELearning.Application.Features.Media.Commands;

public sealed record UploadThumbnailCommand(
    IFormFile File
) : ICommand<UploadResultDto>;

public sealed class UploadThumbnailHandler : ICommandHandler<UploadThumbnailCommand, UploadResultDto>
{
    private readonly IStorageService _storage;

    public UploadThumbnailHandler(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task<Result<UploadResultDto>> HandleAsync(
        UploadThumbnailCommand cmd,
        CancellationToken ct = default)
    {
        await using var stream = cmd.File.OpenReadStream();

        var url = await _storage.UploadAsync(
            stream: stream,
            fileName: cmd.File.FileName,
            folder: "thumbnails",
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
