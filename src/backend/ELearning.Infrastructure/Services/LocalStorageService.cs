using ELearning.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ELearning.Infrastructure.Services;

/// <summary>
/// Implementación de desarrollo — guarda archivos en wwwroot/uploads.
/// 
/// Para cambiar a S3 o Azure en producción:
///   1. Crea S3StorageService o AzureBlobStorageService implementando IStorageService
///   2. En Infrastructure/DependencyInjection.cs cambia el registro:
///      services.AddScoped IStorageService, S3StorageService  (producción)
///      services.AddScoped IStorageService, LocalStorageService  (desarrollo)
///   3. El resto del sistema no requiere ningún cambio.
/// 
/// Estructura en disco:
///   wwwroot/
///   └── uploads/
///       ├── videos/
///       ├── pdfs/
///       └── thumbnails/
/// </summary>
public sealed class LocalStorageService : IStorageService
{
    private readonly string _baseUploadPath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IWebHostEnvironment env, ILogger<LocalStorageService> logger)
    {
        _logger = logger;
        _baseUploadPath = Path.Combine(env.WebRootPath, "uploads");
        // En desarrollo la URL base es el propio servidor
        // En producción esto vendría de configuración apuntando al CDN o bucket
        _baseUrl = "http://localhost:5000/uploads";
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string folder, string contentType, CancellationToken ct = default)
    {
        // Crear la carpeta destino si no existe
        var folderPath = Path.Combine(_baseUploadPath, folder);
        Directory.CreateDirectory(folderPath);

        // Generar nombre único para evitar colisiones
        var uniqueFileName = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920, // 80 KB buffer — eficiente para archivos grandes
            useAsync: true);

        await stream.CopyToAsync(fileStream, ct);

        var url = $"{_baseUrl}/{folder}/{uniqueFileName}";
        _logger.LogInformation("[LocalStorage] Archivo guardado: {FilePath} → {Url}", filePath, url);

        return url;
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var filePath = UrlToFilePath(fileUrl);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("[LocalStorage] Archivo eliminado: {FilePath}", filePath);
        }
        else
        {
            _logger.LogWarning("[LocalStorage] Archivo no encontrado para eliminar: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string fileUrl, CancellationToken ct = default)
    {
        var filePath = UrlToFilePath(fileUrl);
        return Task.FromResult(File.Exists(filePath));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Convierte una URL pública de vuelta a la ruta en disco.
    /// Ejemplo: http://localhost:5000/uploads/videos/abc.mp4
    ///       → wwwroot/uploads/videos/abc.mp4
    /// </summary>
    private string UrlToFilePath(string fileUrl)
    {
        var relativePath = fileUrl.Replace(_baseUrl, string.Empty).TrimStart('/');
        return Path.Combine(_baseUploadPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    /// <summary>
    /// Elimina caracteres no permitidos en nombres de archivo.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(fileName
            .Select(c => invalid.Contains(c) ? '_' : c));

        // Limitar longitud para evitar paths demasiado largos
        return sanitized.Length > 100
            ? sanitized[..100]
            : sanitized;
    }
}