namespace SimpleTextEditor.Blazor.Components;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Komponent wyświetlający podgląd wyrenderowanej treści Markdown jako HTML.
/// </summary>
public partial class PreviewPanel : ComponentBase
{
    /// <summary>
    /// Zawartość HTML do wyświetlenia (skonwertowana z Markdown).
    /// </summary>
    [Parameter]
    public string? HtmlContent { get; set; }
    
    /// <summary>
    /// Komunikat wyświetlany gdy brak treści do podglądu.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "Nothing to preview";
    
    /// <summary>
    /// Dodatkowa klasa CSS dla kontenera panelu podglądu.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}
