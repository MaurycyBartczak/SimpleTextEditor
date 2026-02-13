using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Services;

/// <summary>
/// Domyślny handler uploadu obrazów konwertujący obrazy na data URI w formacie Base64.
/// Obrazy są osadzane bezpośrednio w treści Markdown.
/// </summary>
public class Base64ImageUploadHandler : IImageUploadHandler
{
    /// <inheritdoc />
    public Task<string> UploadAsync(string fileName, byte[] content, string contentType)
    {
        var base64 = Convert.ToBase64String(content);
        var dataUri = $"data:{contentType};base64,{base64}";
        return Task.FromResult(dataUri);
    }
}
