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

        var uniqueFileName = GenerateUniqueFileName(fileName, contentType);

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
    /// Generuje unikalną nazwę pliku za pomocą Guid i oryginalnego rozszerzenia.
    /// Nadpisz, aby dostosować strategię nazewnictwa.
    /// </summary>
    protected virtual string GenerateUniqueFileName(string originalFileName, string contentType)
    {
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(extension))
            extension = GetExtensionFromMime(contentType);

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
        "image/webp",
        "image/svg+xml"
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
        "image/svg+xml" => ".svg",
        _ => ".bin"
    };
}
