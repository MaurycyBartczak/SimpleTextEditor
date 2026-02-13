namespace SimpleTextEditor.Core.Models;

/// <summary>
/// Tryby wyświetlania podglądu w edytorze.
/// </summary>
public enum PreviewMode
{
    /// <summary>
    /// Podgląd nie jest wyświetlany.
    /// </summary>
    None,
    
    /// <summary>
    /// Edytor i podgląd wyświetlane obok siebie.
    /// </summary>
    SideBySide,
    
    /// <summary>
    /// Przełączanie między widokiem edytora a podglądem.
    /// </summary>
    Toggle
}
