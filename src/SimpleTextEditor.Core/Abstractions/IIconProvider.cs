namespace SimpleTextEditor.Core.Abstractions;

/// <summary>
/// Interfejs dostarczający nazwy/klasy ikon dla przycisków paska narzędzi.
/// Pozwala na dostosowanie zestawów ikon (Material Icons, FontAwesome itp.).
/// </summary>
public interface IIconProvider
{
    /// <summary>
    /// Pobiera identyfikator ikony dla podanej nazwy akcji.
    /// </summary>
    /// <param name="actionName">Nazwa akcji (np. "bold", "italic", "heading1").</param>
    /// <returns>Identyfikator ikony (klasa CSS, nazwa ikony lub ścieżka SVG).</returns>
    string GetIcon(string actionName);
    
    /// <summary>
    /// Pobiera link CSS lub instrukcję importu do załadowania czcionki ikon.
    /// </summary>
    /// <returns>Element HTML link lub pusty string jeśli nie jest potrzebny.</returns>
    string GetIconFontLink();
}
