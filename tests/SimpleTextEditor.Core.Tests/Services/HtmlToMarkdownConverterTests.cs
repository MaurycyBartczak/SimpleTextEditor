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
}
