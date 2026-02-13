namespace SimpleTextEditor.Core.Abstractions;

/// <summary>
/// Interfejs do konwersji Markdown na HTML.
/// </summary>
public interface IMarkdownParser
{
    /// <summary>
    /// Konwertuje tekst Markdown na HTML.
    /// </summary>
    /// <param name="markdown">Tekst Markdown do konwersji.</param>
    /// <returns>Reprezentacja HTML tekstu Markdown.</returns>
    string ToHtml(string markdown);
    
    /// <summary>
    /// Konwertuje tekst Markdown na czysty tekst (usuwa całe formatowanie).
    /// </summary>
    /// <param name="markdown">Tekst Markdown do konwersji.</param>
    /// <returns>Czysty tekst bez formatowania.</returns>
    string ToPlainText(string markdown);
}
