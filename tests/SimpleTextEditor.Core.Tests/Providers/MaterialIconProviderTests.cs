using SimpleTextEditor.Core.Providers;

namespace SimpleTextEditor.Core.Tests.Providers;

public class MaterialIconProviderTests
{
    private readonly MaterialIconProvider _sut = new();

    [Theory]
    [InlineData("bold", "format_bold")]
    [InlineData("italic", "format_italic")]
    [InlineData("strikethrough", "strikethrough_s")]
    [InlineData("heading1", "format_h1")]
    [InlineData("heading2", "format_h2")]
    [InlineData("heading3", "format_h3")]
    [InlineData("bulletList", "format_list_bulleted")]
    [InlineData("numberedList", "format_list_numbered")]
    [InlineData("link", "link")]
    [InlineData("image", "image")]
    [InlineData("undo", "undo")]
    [InlineData("redo", "redo")]
    public void GetIcon_KnownAction_ReturnsCorrectIcon(string actionName, string expectedIcon)
    {
        Assert.Equal(expectedIcon, _sut.GetIcon(actionName));
    }

    [Fact]
    public void GetIcon_UnknownAction_ReturnsActionNameAsFallback()
    {
        Assert.Equal("unknownAction", _sut.GetIcon("unknownAction"));
    }

    [Fact]
    public void GetIconFontLink_ReturnsGoogleFontsLink()
    {
        var link = _sut.GetIconFontLink();
        
        Assert.Contains("fonts.googleapis.com", link);
        Assert.Contains("Material+Symbols", link);
        Assert.Contains("<link", link);
    }
}
