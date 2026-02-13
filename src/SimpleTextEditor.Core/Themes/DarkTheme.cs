using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Themes;

/// <summary>
/// Ciemny motyw edytora.
/// </summary>
public class DarkTheme : IEditorTheme
{
    /// <inheritdoc />
    public string Name => "dark";
    
    /// <inheritdoc />
    public string ContainerClass => "ste-container ste-dark";
    
    /// <inheritdoc />
    public string ToolbarClass => "ste-toolbar ste-toolbar-dark";
    
    /// <inheritdoc />
    public string EditorClass => "ste-editor ste-editor-dark";
    
    /// <inheritdoc />
    public string PreviewClass => "ste-preview ste-preview-dark";
    
    /// <inheritdoc />
    public string AdditionalStyles => string.Empty;
}
