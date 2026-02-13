using SimpleTextEditor.Core.Services;

namespace SimpleTextEditor.Core.Tests.Services;

public class MarkdownServiceTests
{
    private readonly MarkdownService _sut = new();

    [Fact]
    public void ToHtml_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.ToHtml(string.Empty));
    }

    [Fact]
    public void ToHtml_NullString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.ToHtml(null!));
    }

    [Theory]
    [InlineData("# Heading", "<h1")]
    [InlineData("## Heading 2", "<h2")]
    [InlineData("**bold**", "<strong>bold</strong>")]
    [InlineData("*italic*", "<em>italic</em>")]
    [InlineData("- item1", "<li>item1</li>")]
    public void ToHtml_BasicMarkdown_ContainsExpectedHtml(string markdown, string expectedFragment)
    {
        var result = _sut.ToHtml(markdown);
        Assert.Contains(expectedFragment, result);
    }

    [Fact]
    public void ToHtml_Table_RendersTableHtml()
    {
        var markdown = "| H1 | H2 |\n|---|---|\n| A | B |";
        var result = _sut.ToHtml(markdown);
        Assert.Contains("<table", result);
        Assert.Contains("<td>A</td>", result);
    }

    [Fact]
    public void ToHtml_Link_RendersAnchorTag()
    {
        var result = _sut.ToHtml("[test](https://example.com)");
        Assert.Contains("<a", result);
        Assert.Contains("https://example.com", result);
    }

    [Fact]
    public void ToHtml_CodeBlock_RendersPreTag()
    {
        var markdown = "```csharp\nvar x = 1;\n```";
        var result = _sut.ToHtml(markdown);
        Assert.Contains("<code", result);
    }

    [Fact]
    public void ToPlainText_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.ToPlainText(string.Empty));
    }

    [Fact]
    public void ToPlainText_NullString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.ToPlainText(null!));
    }

    [Theory]
    [InlineData("**bold text**", "bold text")]
    [InlineData("*italic*", "italic")]
    [InlineData("# Heading", "Heading")]
    public void ToPlainText_FormattedMarkdown_ReturnsPlainText(string markdown, string expectedText)
    {
        var result = _sut.ToPlainText(markdown).Trim();
        Assert.Contains(expectedText, result);
    }
}
