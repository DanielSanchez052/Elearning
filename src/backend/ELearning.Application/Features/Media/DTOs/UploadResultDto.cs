namespace ELearning.Application.Features.Media.DTOs;

public sealed record UploadResultDto(
    string Url,
    string FileName,
    long FileSizeBytes,
    string ContentType
);

