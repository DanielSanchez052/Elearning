namespace ELearning.Domain.Interfaces.Services;

/// <summary>
/// Contrato de almacenamiento de archivos.
/// Implementaciones: LocalStorageService (desarrollo), S3StorageService, AzureBlobStorageService.
/// El resto del sistema nunca conoce qué implementación está activa.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Sube un archivo y retorna la URL pública permanente.
    /// </summary>
    /// <param name="stream">Contenido del archivo.</param>
    /// <param name="fileName">Nombre original del archivo.</param>
    /// <param name="folder">Carpeta destino: "videos", "pdfs", "thumbnails".</param>
    /// <param name="contentType">MIME type del archivo.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>URL pública del archivo subido.</returns>
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folder,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Elimina un archivo por su URL pública.
    /// </summary>
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un archivo existe por su URL pública.
    /// </summary>
    Task<bool> ExistsAsync(string fileUrl, CancellationToken ct = default);
}