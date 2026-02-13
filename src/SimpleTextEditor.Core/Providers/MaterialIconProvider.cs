using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Providers;

/// <summary>
/// Dostawca ikon wykorzystujący Google Material Symbols.
/// Używa wariantów wypełnionych dla bardziej wyrazistego wyglądu.
/// </summary>
public class MaterialIconProvider : IIconProvider
{
    private static readonly Dictionary<string, string> IconMap = new()
    {
        // Formatowanie tekstu
        ["bold"] = "format_bold",
        ["italic"] = "format_italic",
        ["strikethrough"] = "strikethrough_s",
        
        // Nagłówki
        ["heading1"] = "format_h1",
        ["heading2"] = "format_h2",
        ["heading3"] = "format_h3",
        
        // Listy
        ["bulletList"] = "format_list_bulleted",
        ["numberedList"] = "format_list_numbered",
        ["quote"] = "format_quote",
        
        // Wyrównanie
        ["alignLeft"] = "format_align_left",
        ["alignCenter"] = "format_align_center",
        ["alignRight"] = "format_align_right",
        
        // Wstawianie
        ["link"] = "link",
        ["image"] = "image",
        ["table"] = "table_chart",
        ["code"] = "code",
        ["codeBlock"] = "terminal",
        ["horizontalRule"] = "horizontal_rule",
        
        // Akcje
        ["undo"] = "undo",
        ["redo"] = "redo",
        ["preview"] = "visibility",
        ["fullscreen"] = "fullscreen",
        ["switchMode"] = "edit_note",
        
        // Dodatkowe
        ["copy"] = "content_copy",
        ["cut"] = "content_cut",
        ["paste"] = "content_paste",
        ["clear"] = "clear",
        ["help"] = "help",
        ["settings"] = "settings"
    };
    
    /// <inheritdoc />
    public string GetIcon(string actionName)
    {
        return IconMap.TryGetValue(actionName, out var icon) ? icon : actionName;
    }
    
    /// <inheritdoc />
    public string GetIconFontLink()
    {
        return """<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@24,400,0,0" rel="stylesheet" />""";
    }
}
