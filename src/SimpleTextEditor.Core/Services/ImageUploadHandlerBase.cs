using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Services;

/// <summary>
/// Abstrakcyjna klasa bazowa dla handlerów uploadu obrazów.
/// Zapewnia wbudowaną walidację rozmiaru pliku, typu MIME oraz generowanie unikalnych nazw plików.
/// Dziedzicz z tej klasy i zaimplementuj <see cref="SaveAsync"/> z logiką zapisu.
/// </summary>
public abstract class ImageUploadHandlerBase : IImageUploadHandler
{
    /// <inheritdoc />
    public async Task<string> UploadAsync(string fileName, byte[] content, string contentType)
    {
        if (MaxFileSizeBytes > 0 && content.Length > MaxFileSizeBytes)
            throw new InvalidOperationException(
                $"Rozmiar pliku ({content.Length} bajtów) przekracza maksymalny dozwolony rozmiar ({MaxFileSizeBytes} bajtów).");

        if (!AllowedContentTypes.Contains(contentType))
            throw new InvalidOperationException(
                $"Typ zawartości '{contentType}' jest niedozwolony. Dozwolone typy: {string.Join(", ", AllowedContentTypes)}.");

        // Waliduj magic bytes — zweryfikowany MIME musi zgadzać się z deklarowanym
        var detectedMime = DetectMimeFromMagicBytes(content);
        if (detectedMime == null || !string.Equals(detectedMime, contentType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Sygnatura pliku nie zgadza się z deklarowanym typem '{contentType}'. Wykryto: '{detectedMime ?? "nieznany"}'.");

        // Generuj rozszerzenie na podstawie zweryfikowanego MIME, nie nazwy od klienta
        var uniqueFileName = GenerateUniqueFileName(contentType);

        return await SaveAsync(uniqueFileName, content, contentType);
    }

    /// <summary>
    /// Zapisuje obraz do docelowego miejsca przechowywania i zwraca URL.
    /// Wywoływana po pomyślnej walidacji. Nadpisz tę metodę swoją logiką zapisu.
    /// </summary>
    /// <param name="uniqueFileName">Wygenerowana unikalna nazwa pliku (np. "a1b2c3d4.png").</param>
    /// <param name="content">Zawartość pliku jako tablica bajtów.</param>
    /// <param name="contentType">Typ MIME obrazu.</param>
    /// <returns>URL lub ścieżka do zapisanego obrazu.</returns>
    protected abstract Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType);

    /// <summary>
    /// Generuje unikalną nazwę pliku na podstawie zweryfikowanego typu MIME.
    /// Rozszerzenie jest brane z MIME, nie z nazwy pliku od klienta.
    /// </summary>
    protected virtual string GenerateUniqueFileName(string contentType)
    {
        var extension = GetExtensionFromMime(contentType);
        return $"{Guid.NewGuid()}{extension}";
    }

    /// <inheritdoc />
    public virtual long MaxFileSizeBytes => 10 * 1024 * 1024; // 10 MB

    /// <inheritdoc />
    public virtual IReadOnlyList<string> AllowedContentTypes => new[]
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    /// <summary>
    /// Mapuje typ MIME na rozszerzenie pliku.
    /// </summary>
    private static string GetExtensionFromMime(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        _ => ".bin"
    };

    /// <summary>
    /// Wykrywa typ MIME na podstawie magic bytes (sygnatury pliku).
    /// Zwraca null jeśli sygnatura jest nieznana.
    /// </summary>
    internal static string? DetectMimeFromMagicBytes(byte[] content)
    {
        if (content.Length < 4)
            return null;

        // JPEG: FF D8 FF
        if (content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
            return "image/jpeg";

        // PNG: 89 50 4E 47
        if (content[0] == 0x89 && content[1] == 0x50 && content[2] == 0x4E && content[3] == 0x47)
            return "image/png";

        // GIF: 47 49 46 38
        if (content[0] == 0x47 && content[1] == 0x49 && content[2] == 0x46 && content[3] == 0x38)
            return "image/gif";

        // WebP: RIFF....WEBP
        if (content.Length >= 12 &&
            content[0] == 0x52 && content[1] == 0x49 && content[2] == 0x46 && content[3] == 0x46 &&
            content[8] == 0x57 && content[9] == 0x45 && content[10] == 0x42 && content[11] == 0x50)
            return "image/webp";

        return null;
    }
}
