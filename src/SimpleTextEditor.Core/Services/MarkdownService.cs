using Markdig;
using SimpleTextEditor.Core.Abstractions;

namespace SimpleTextEditor.Core.Services;

/// <summary>
/// Implementacja parsera Markdown przy użyciu biblioteki Markdig.
/// </summary>
public class MarkdownService : IMarkdownParser
{
    private readonly MarkdownPipeline _pipeline;
    
    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseAutoLinks()
            .UseTaskLists()
            .UsePipeTables()
            .UseGridTables()
            .UseFootnotes()
            .UseAutoIdentifiers()
            .DisableHtml()
            .Build();
    }

    /// <inheritdoc />
    public string ToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        var rawHtml = Markdown.ToHtml(markdown, _pipeline);
        return HtmlSanitizationService.Sanitize(rawHtml);
    }
    
    /// <inheritdoc />
    public string ToPlainText(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;
        
        return Markdown.ToPlainText(markdown, _pipeline);
    }
}
