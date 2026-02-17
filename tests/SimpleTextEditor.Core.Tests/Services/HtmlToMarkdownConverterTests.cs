using SimpleTextEditor.Core.Services;

namespace SimpleTextEditor.Core.Tests.Services;

public class HtmlToMarkdownConverterTests
{
    private readonly HtmlToMarkdownConverter _sut = new();

    [Fact]
    public void Convert_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.Convert(string.Empty));
    }

    [Fact]
    public void Convert_NullString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.Convert(null!));
    }

    [Fact]
    public void Convert_StrongTag_ReturnsBoldMarkdown()
    {
        var result = _sut.Convert("<strong>bold</strong>");
        Assert.Contains("**bold**", result);
    }

    [Fact]
    public void Convert_EmTag_ReturnsItalicMarkdown()
    {
        var result = _sut.Convert("<em>italic</em>");
        Assert.Contains("*italic*", result);
    }

    [Fact]
    public void Convert_H1Tag_ReturnsHeading()
    {
        var result = _sut.Convert("<h1>Title</h1>");
        Assert.Contains("# Title", result);
    }

    [Fact]
    public void Convert_AnchorTag_ReturnsLink()
    {
        var result = _sut.Convert("<a href=\"https://example.com\">link</a>");
        Assert.Contains("[link](https://example.com)", result);
    }

    [Fact]
    public void Convert_UnorderedList_ReturnsBulletList()
    {
        var result = _sut.Convert("<ul><li>item1</li><li>item2</li></ul>");
        Assert.Contains("item1", result);
        Assert.Contains("item2", result);
    }

    [Fact]
    public void Convert_Paragraph_ReturnsTrimmedText()
    {
        var result = _sut.Convert("<p>Hello World</p>");
        Assert.Equal("Hello World", result);
    }

    // === Testy regresyjne XSS ===

    [Fact]
    public void Convert_ScriptTag_IsRemoved()
    {
        var result = _sut.Convert("<p>safe</p><script>alert(1)</script>");
        Assert.DoesNotContain("<script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", result);
        Assert.Contains("safe", result);
    }

    [Fact]
    public void Convert_ImgOnerror_IsNeutralized()
    {
        var result = _sut.Convert("<img src=\"x\" onerror=\"alert(1)\">");
        Assert.DoesNotContain("onerror", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Convert_JavascriptHref_IsNeutralized()
    {
        var result = _sut.Convert("<a href=\"javascript:alert(1)\">click</a>");
        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Convert_UnknownTags_AreDropped()
    {
        var result = _sut.Convert("<custom-tag>payload</custom-tag><p>safe</p>");
        Assert.DoesNotContain("<custom-tag", result);
        Assert.Contains("safe", result);
    }

    [Fact]
    public void Convert_SafeHtml_PreservesContent()
    {
        var html = "<h1>Title</h1><p><strong>bold</strong> and <em>italic</em></p>";
        var result = _sut.Convert(html);
        Assert.Contains("# Title", result);
        Assert.Contains("**bold**", result);
        Assert.Contains("*italic*", result);
    }
}
