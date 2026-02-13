namespace SimpleTextEditor.Core.Models;

/// <summary>
/// Definiuje tryb edycji dla edytora Markdown.
/// </summary>
public enum EditorMode
{
    /// <summary>
    /// Tryb edycji źródła Markdown — użytkownik widzi i edytuje surową składnię Markdown.
    /// </summary>
    Markdown,
    
    /// <summary>
    /// Tryb edycji WYSIWYG — użytkownik widzi sformatowany tekst i edytuje wizualnie.
    /// </summary>
    Wysiwyg
}
