namespace SimpleTextEditor.Core.Abstractions;

/// <summary>
/// Interfejs konfiguracji motywu edytora.
/// </summary>
public interface IEditorTheme
{
    /// <summary>
    /// Pobiera nazwę motywu (np. "light", "dark").
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Pobiera klasę CSS do zastosowania na kontenerze edytora.
    /// </summary>
    string ContainerClass { get; }
    
    /// <summary>
    /// Pobiera klasę CSS do zastosowania na pasku narzędzi.
    /// </summary>
    string ToolbarClass { get; }
    
    /// <summary>
    /// Pobiera klasę CSS do zastosowania na polu tekstowym edytora.
    /// </summary>
    string EditorClass { get; }
    
    /// <summary>
    /// Pobiera klasę CSS do zastosowania na panelu podglądu.
    /// </summary>
    string PreviewClass { get; }
    
    /// <summary>
    /// Pobiera dodatkowe style CSS do wstrzyknięcia.
    /// </summary>
    string AdditionalStyles { get; }
}
