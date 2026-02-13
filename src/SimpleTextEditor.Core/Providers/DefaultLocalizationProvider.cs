using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Providers;

/// <summary>
/// Domyślny dostawca lokalizacji z tłumaczeniami na angielski i polski.
/// </summary>
public class DefaultLocalizationProvider : ILocalizationProvider
{
    private string _currentLanguage = "en";
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private Dictionary<string, string> _customTranslations = new();
    
    public DefaultLocalizationProvider()
    {
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new()
            {
                // Formatowanie tekstu
                ["bold"] = "Bold",
                ["italic"] = "Italic",
                ["strikethrough"] = "Strikethrough",
                
                // Nagłówki
                ["heading1"] = "Heading 1",
                ["heading2"] = "Heading 2",
                ["heading3"] = "Heading 3",
                
                // Listy
                ["bulletList"] = "Bullet List",
                ["numberedList"] = "Numbered List",
                ["quote"] = "Quote",
                
                // Wyrównanie
                ["alignLeft"] = "Align Left",
                ["alignCenter"] = "Align Center",
                ["alignRight"] = "Align Right",
                
                // Wstawianie
                ["link"] = "Insert Link",
                ["image"] = "Insert Image",
                ["table"] = "Insert Table",
                ["code"] = "Inline Code",
                ["codeBlock"] = "Code Block",
                ["horizontalRule"] = "Horizontal Rule",
                
                // Akcje
                ["undo"] = "Undo",
                ["redo"] = "Redo",
                ["preview"] = "Toggle Preview",
                ["fullscreen"] = "Fullscreen",
                ["switchMode"] = "Switch Mode",
                
                // Interfejs
                ["placeholder"] = "Start typing...",
                ["previewTitle"] = "Preview",
                ["editorTitle"] = "Editor",
                ["noPreview"] = "Nothing to preview"
            },
            ["pl"] = new()
            {
                // Formatowanie tekstu
                ["bold"] = "Pogrubienie",
                ["italic"] = "Kursywa",
                ["strikethrough"] = "Przekreślenie",
                
                // Nagłówki
                ["heading1"] = "Nagłówek 1",
                ["heading2"] = "Nagłówek 2",
                ["heading3"] = "Nagłówek 3",
                
                // Listy
                ["bulletList"] = "Lista punktowana",
                ["numberedList"] = "Lista numerowana",
                ["quote"] = "Cytat",
                
                // Wyrównanie
                ["alignLeft"] = "Wyrównaj do lewej",
                ["alignCenter"] = "Wyrównaj do środka",
                ["alignRight"] = "Wyrównaj do prawej",
                
                // Wstawianie
                ["link"] = "Wstaw link",
                ["image"] = "Wstaw obraz",
                ["table"] = "Wstaw tabelę",
                ["code"] = "Kod w linii",
                ["codeBlock"] = "Blok kodu",
                ["horizontalRule"] = "Linia pozioma",
                
                // Akcje
                ["undo"] = "Cofnij",
                ["redo"] = "Ponów",
                ["preview"] = "Przełącz podgląd",
                ["fullscreen"] = "Pełny ekran",
                ["switchMode"] = "Zmień tryb",
                
                // Interfejs
                ["placeholder"] = "Zacznij pisać...",
                ["previewTitle"] = "Podgląd",
                ["editorTitle"] = "Edytor",
                ["noPreview"] = "Brak podglądu"
            }
        };
    }
    
    /// <inheritdoc />
    public string CurrentLanguage => _currentLanguage;
    
    /// <inheritdoc />
    public string Get(string key)
    {
        // Najpierw sprawdź niestandardowe tłumaczenia
        if (_customTranslations.TryGetValue(key, out var custom))
            return custom;
        
        // Potem sprawdź tłumaczenia dla aktualnego języka
        if (_translations.TryGetValue(_currentLanguage, out var langDict) &&
            langDict.TryGetValue(key, out var translation))
            return translation;
        
        // Awaryjnie użyj angielskiego
        if (_translations.TryGetValue("en", out var enDict) &&
            enDict.TryGetValue(key, out var enTranslation))
            return enTranslation;
        
        // W ostateczności zwróć sam klucz
        return key;
    }
    
    /// <inheritdoc />
    public IEnumerable<string> GetAvailableLanguages() => _translations.Keys;
    
    /// <inheritdoc />
    public void SetLanguage(string languageCode)
    {
        if (_translations.ContainsKey(languageCode))
            _currentLanguage = languageCode;
    }
    
    /// <inheritdoc />
    public void AddTranslations(IDictionary<string, string> translations)
    {
        foreach (var (key, value) in translations)
        {
            _customTranslations[key] = value;
        }
    }
}
