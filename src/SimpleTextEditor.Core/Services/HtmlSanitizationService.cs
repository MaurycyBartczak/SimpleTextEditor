using Ganss.Xss;

namespace SimpleTextEditor.Core.Services;

/// <summary>
/// Serwis sanitizacji HTML oparty o allowlistę tagów, atrybutów i protokołów URL.
/// Neutralizuje wektory XSS (script, onerror, javascript: itp.) zachowując bezpieczne formatowanie.
/// </summary>
public static class HtmlSanitizationService
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        // Dozwolone tagi — pełne pokrycie Markdig output + WYSIWYG
        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            // Blokowe
            "h1", "h2", "h3", "h4", "h5", "h6",
            "p", "div", "blockquote", "pre",
            "ul", "ol", "li",
            "table", "thead", "tbody", "tfoot", "tr", "th", "td",
            "hr", "br",
            // Inline
            "a", "strong", "b", "em", "i", "u", "s", "del", "ins",
            "code", "mark", "sub", "sup", "span",
            "img",
            // Detale
            "details", "summary",
            // Definicje
            "dl", "dt", "dd",
            // Footnotes (Markdig)
            "section", "aside",
            "figure", "figcaption",
            "abbr", "cite", "dfn", "kbd", "samp", "var",
            // Listy zadań
            "input",
            // Kolgroup (tabele)
            "col", "colgroup", "caption"
        })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        // Dozwolone atrybuty
        sanitizer.AllowedAttributes.Clear();
        foreach (var attr in new[]
        {
            "href", "src", "alt", "title", "width", "height",
            "class", "id", "name",
            "colspan", "rowspan", "scope", "align", "valign",
            "start", "reversed", "type",
            // Checkbox w tasklistach
            "checked", "disabled",
            // Obrazki
            "loading",
            // Abbr
            "data-footnote-id", "data-footnote-backref",
            // Style ograniczone (sanitizer filtruje niebezpieczne wartości CSS)
            "style"
        })
        {
            sanitizer.AllowedAttributes.Add(attr);
        }

        // Dozwolone protokoły URL — tylko bezpieczne
        sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https", "mailto", "tel" })
        {
            sanitizer.AllowedSchemes.Add(scheme);
        }

        // Dozwolone właściwości CSS (minimalne, bezpieczne)
        sanitizer.AllowedCssProperties.Clear();
        foreach (var prop in new[]
        {
            "text-align", "width", "height", "max-width", "max-height",
            "color", "background-color", "font-weight", "font-style",
            "text-decoration", "margin", "padding", "border"
        })
        {
            sanitizer.AllowedCssProperties.Add(prop);
        }

        // Zezwól na data: URI tylko dla src obrazków (base64 inline images)
        sanitizer.AllowedAtRules.Clear();
        sanitizer.AllowDataAttributes = false;

        // Dodaj data: jako dozwolony schemat dla src obrazków
        sanitizer.AllowedSchemes.Add("data");

        // Filtruj URL callback — data: URI dozwolone tylko dla obrazków (img src)
        sanitizer.FilterUrl += (sender, args) =>
        {
            if (args.OriginalUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var isImgSrc = args.Tag?.TagName.Equals("IMG", StringComparison.OrdinalIgnoreCase) == true;
                if (!isImgSrc)
                {
                    args.SanitizedUrl = string.Empty;
                }
                else if (!args.OriginalUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                {
                    // Tylko data:image/* jest bezpieczne
                    args.SanitizedUrl = string.Empty;
                }
            }
        };

        return sanitizer;
    }

    /// <summary>
    /// Sanityzuje HTML usuwając niebezpieczne elementy (script, event handlers, javascript: itp.).
    /// Zachowuje bezpieczne formatowanie.
    /// </summary>
    public static string Sanitize(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        return Sanitizer.Sanitize(html);
    }
}
