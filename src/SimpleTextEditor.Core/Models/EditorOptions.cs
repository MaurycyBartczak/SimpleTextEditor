using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Models;

/// <summary>
/// Opcje konfiguracyjne edytora Markdown.
/// </summary>
public class EditorOptions
{
    /// <summary>
    /// Tekst zastępczy wyświetlany gdy edytor jest pusty.
    /// </summary>
    public string Placeholder { get; set; } = "";
    
    /// <summary>
    /// Tryb podglądu (None, SideBySide, Toggle).
    /// </summary>
    public PreviewMode PreviewMode { get; set; } = PreviewMode.SideBySide;
    
    /// <summary>
    /// Nazwa motywu ("light" lub "dark").
    /// </summary>
    public string Theme { get; set; } = "light";
    
    /// <summary>
    /// Kod języka do lokalizacji (np. "en", "pl").
    /// </summary>
    public string Language { get; set; } = "en";
    
    /// <summary>
    /// Niestandardowe elementy paska narzędzi. Jeśli null, używane są domyślne.
    /// </summary>
    public IReadOnlyList<ToolbarItem>? ToolbarItems { get; set; }
    
    /// <summary>
    /// Niestandardowa klasa CSS dla kontenera edytora.
    /// </summary>
    public string? CssClass { get; set; }
    
    /// <summary>
    /// Niestandardowy dostawca ikon. Jeśli null, używany jest MaterialIconProvider.
    /// </summary>
    public IIconProvider? IconProvider { get; set; }
    
    /// <summary>
    /// Niestandardowy dostawca lokalizacji.
    /// </summary>
    public ILocalizationProvider? LocalizationProvider { get; set; }
    
    /// <summary>
    /// Niestandardowy motyw.
    /// </summary>
    public IEditorTheme? EditorTheme { get; set; }
    
    /// <summary>
    /// Niestandardowe tłumaczenia do nadpisania lub dodania.
    /// </summary>
    public IDictionary<string, string>? CustomTranslations { get; set; }
    
    /// <summary>
    /// Minimalna wysokość edytora w pikselach.
    /// </summary>
    public int MinHeight { get; set; } = 300;
    
    /// <summary>
    /// Maksymalna wysokość edytora w pikselach (0 = brak limitu).
    /// </summary>
    public int MaxHeight { get; set; } = 0;
    
    /// <summary>
    /// Jeśli true, wyświetla numery linii w edytorze.
    /// </summary>
    public bool ShowLineNumbers { get; set; } = false;
    
    /// <summary>
    /// Jeśli true, edytor jest tylko do odczytu.
    /// </summary>
    public bool ReadOnly { get; set; } = false;
}
