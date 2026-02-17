using ReverseMarkdown;
using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Services;

/// <summary>
/// Konwerter HTML na Markdown przy użyciu biblioteki ReverseMarkdown.
/// </summary>
public class HtmlToMarkdownConverter : IHtmlToMarkdownConverter
{
    private readonly Converter _converter;
    
    public HtmlToMarkdownConverter()
    {
        var config = new ReverseMarkdown.Config
        {
            UnknownTags = Config.UnknownTagsOption.Drop,
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true
        };

        _converter = new Converter(config);
    }

    /// <inheritdoc />
    public string Convert(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Sanityzuj HTML przed konwersją — usuń niebezpieczne tagi/atrybuty
        var sanitized = HtmlSanitizationService.Sanitize(html);
        return _converter.Convert(sanitized).Trim();
    }
}
