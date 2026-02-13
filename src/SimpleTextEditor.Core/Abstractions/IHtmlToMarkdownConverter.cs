namespace SimpleTextEditor.Core.Abstractions;

/// <summary>
/// Interfejs do konwersji treści HTML na Markdown.
/// Używany przy przełączaniu z trybu WYSIWYG na tryb Markdown.
/// </summary>
public interface IHtmlToMarkdownConverter
{
    /// <summary>
    /// Konwertuje treść HTML na składnię Markdown.
    /// </summary>
    /// <param name="html">Treść HTML do konwersji.</param>
    /// <returns>Reprezentacja Markdown treści HTML.</returns>
    string Convert(string html);
}
