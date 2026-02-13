using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Themes;

/// <summary>
/// Jasny motyw edytora.
/// </summary>
public class LightTheme : IEditorTheme
{
    /// <inheritdoc />
    public string Name => "light";
    
    /// <inheritdoc />
    public string ContainerClass => "ste-container ste-light";
    
    /// <inheritdoc />
    public string ToolbarClass => "ste-toolbar ste-toolbar-light";
    
    /// <inheritdoc />
    public string EditorClass => "ste-editor ste-editor-light";
    
    /// <inheritdoc />
    public string PreviewClass => "ste-preview ste-preview-light";
    
    /// <inheritdoc />
    public string AdditionalStyles => string.Empty;
}
