namespace SimpleTextEditor.Core.Abstractions;

/// <summary>
/// Interfejs do obsługi uploadu obrazów w edytorze.
/// Zaimplementuj ten interfejs, aby dostosować miejsce i sposób przechowywania obrazów.
/// </summary>
public interface IImageUploadHandler
{
    /// <summary>
    /// Obsługuje przesłany obraz i zwraca URL do użycia w edytorze.
    /// </summary>
    /// <param name="fileName">Oryginalna nazwa pliku przesłanego obrazu.</param>
    /// <param name="content">Zawartość pliku jako tablica bajtów.</param>
    /// <param name="contentType">Typ MIME obrazu (np. "image/png").</param>
    /// <returns>URL lub data URI do wstawienia w edytorze.</returns>
    Task<string> UploadAsync(string fileName, byte[] content, string contentType);
    
    /// <summary>
    /// Maksymalny dozwolony rozmiar pliku w bajtach. Zwróć 0 dla braku limitu.
    /// </summary>
    long MaxFileSizeBytes => 10 * 1024 * 1024; // 10 MB domyślnie
    
    /// <summary>
    /// Lista dozwolonych typów MIME dla obrazów.
    /// </summary>
    IReadOnlyList<string> AllowedContentTypes => new[]
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml"
    };
}
