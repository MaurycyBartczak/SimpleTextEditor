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

    // === Testy regresyjne XSS ===

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<SCRIPT>alert(1)</SCRIPT>")]
    public void ToHtml_ScriptTag_IsNeutralized(string payload)
    {
        var result = _sut.ToHtml(payload);
        Assert.DoesNotContain("<script", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToHtml_ImgOnerror_IsNeutralized()
    {
        var result = _sut.ToHtml("<img src=x onerror=alert(1)>");
        // DisableHtml() escapuje raw HTML — nie ma atrybutu onerror w DOM
        Assert.DoesNotContain("<img", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToHtml_JavascriptLink_IsNeutralized()
    {
        var result = _sut.ToHtml("[click](javascript:alert(1))");
        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToHtml_OnClickAttribute_IsNeutralized()
    {
        var result = _sut.ToHtml("<div onclick=\"alert(1)\">test</div>");
        // DisableHtml() escapuje raw HTML — onclick nie jest atrybutem DOM
        Assert.DoesNotContain("<div onclick", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToHtml_IframeTag_IsRemoved()
    {
        var result = _sut.ToHtml("<iframe src=\"https://evil.com\"></iframe>");
        Assert.DoesNotContain("<iframe", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToHtml_SafeMarkdown_PreservesFormatting()
    {
        // Upewniamy się że sanitizacja nie łamie normalnego contentu
        var markdown = "# Title\n\n**bold** and *italic*\n\n- list item\n\n[link](https://safe.com)";
        var result = _sut.ToHtml(markdown);
        Assert.Contains("<h1", result);
        Assert.Contains("<strong>bold</strong>", result);
        Assert.Contains("<em>italic</em>", result);
        Assert.Contains("<li>", result);
        Assert.Contains("https://safe.com", result);
    }
}
