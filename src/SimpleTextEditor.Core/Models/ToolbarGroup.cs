namespace SimpleTextEditor.Core.Models;

/// <summary>
/// Reprezentuje grupę powiązanych elementów paska narzędzi.
/// </summary>
public class ToolbarGroup
{
    /// <summary>
    /// Nazwa/identyfikator grupy.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Elementy w tej grupie.
    /// </summary>
    public required IReadOnlyList<ToolbarItem> Items { get; init; }
}
