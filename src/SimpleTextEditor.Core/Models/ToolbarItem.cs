namespace SimpleTextEditor.Core.Models;

/// <summary>
/// Reprezentuje pojedynczy przycisk lub akcję paska narzędzi.
/// </summary>
public class ToolbarItem
{
    /// <summary>
    /// Unikalny identyfikator akcji (np. "bold", "italic", "heading1").
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Nazwa/klucz ikony do rozwiązania przez IIconProvider.
    /// </summary>
    public required string Icon { get; init; }
    
    /// <summary>
    /// Klucz lokalizacji dla podpowiedzi (tooltip).
    /// </summary>
    public required string TooltipKey { get; init; }
    
    /// <summary>
    /// Składnia Markdown wstawiana przed zaznaczonym tekstem.
    /// </summary>
    public string? MarkdownBefore { get; init; }
    
    /// <summary>
    /// Składnia Markdown wstawiana po zaznaczonym tekście.
    /// </summary>
    public string? MarkdownAfter { get; init; }
    
    /// <summary>
    /// Jeśli true, element jest separatorem (nie przyciskiem).
    /// </summary>
    public bool IsSeparator { get; init; }
    
    /// <summary>
    /// Jeśli true, wstawia nową linię przed składnią.
    /// </summary>
    public bool NewLineBefore { get; init; }
    
    /// <summary>
    /// Skrót klawiaturowy (np. "Ctrl+B").
    /// </summary>
    public string? Shortcut { get; init; }
    
    /// <summary>
    /// Tworzy element separatora.
    /// </summary>
    public static ToolbarItem Separator => new()
    {
        Id = "separator",
        Icon = "",
        TooltipKey = "",
        IsSeparator = true
    };
}
