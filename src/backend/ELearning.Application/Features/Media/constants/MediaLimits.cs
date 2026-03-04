namespace ELearning.Application.Features.Media.constants;

internal static class MediaLimits
{
    public const long VideoMaxBytes = 500L * 1024 * 1024; // 500 MB
    public const long DocumentMaxBytes = 25L * 1024 * 1024; // 25 MB
    public const long ThumbnailMaxBytes = 25L * 1024 * 1024; // 25 MB

    public static readonly string[] VideoContentTypes = ["video/mp4"];
    public static readonly string[] DocumentContentTypes = ["application/pdf"];
    public static readonly string[] ThumbnailContentTypes = ["image/jpeg", "image/png", "image/webp"];
}
