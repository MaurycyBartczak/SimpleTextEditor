using SimpleTextEditor.Core.Models;

namespace SimpleTextEditor.Core.Tests.Models;

public class ToolbarItemsTests
{
    [Fact]
    public void Default_ContainsItems()
    {
        var items = ToolbarItems.Default;
        Assert.NotEmpty(items);
    }

    [Fact]
    public void Default_ContainsBoldItem()
    {
        var items = ToolbarItems.Default;
        Assert.Contains(items, i => i.Id == "bold");
    }

    [Fact]
    public void Default_ContainsSeparators()
    {
        var items = ToolbarItems.Default;
        Assert.Contains(items, i => i.IsSeparator);
    }

    [Fact]
    public void Bold_HasCorrectMarkdownSyntax()
    {
        var bold = ToolbarItems.Bold;
        Assert.Equal("bold", bold.Id);
        Assert.Equal("**", bold.MarkdownBefore);
        Assert.Equal("**", bold.MarkdownAfter);
        Assert.Equal("Ctrl+B", bold.Shortcut);
    }

    [Fact]
    public void Italic_HasCorrectMarkdownSyntax()
    {
        var italic = ToolbarItems.Italic;
        Assert.Equal("italic", italic.Id);
        Assert.Equal("*", italic.MarkdownBefore);
        Assert.Equal("*", italic.MarkdownAfter);
        Assert.Equal("Ctrl+I", italic.Shortcut);
    }

    [Fact]
    public void Heading1_HasNewLineBeforeFlag()
    {
        var h1 = ToolbarItems.Heading1;
        Assert.Equal("# ", h1.MarkdownBefore);
        Assert.True(h1.NewLineBefore);
        Assert.Null(h1.MarkdownAfter);
    }

    [Fact]
    public void Separator_IsSeparatorTrue()
    {
        var sep = ToolbarItem.Separator;
        Assert.True(sep.IsSeparator);
        Assert.Equal("separator", sep.Id);
    }

    [Fact]
    public void ToolbarItemsSeparator_DelegatesToToolbarItem()
    {
        var sep = ToolbarItems.Separator;
        Assert.True(sep.IsSeparator);
    }

    [Fact]
    public void Link_HasCorrectMarkdownWrap()
    {
        var link = ToolbarItems.Link;
        Assert.Equal("[", link.MarkdownBefore);
        Assert.Equal("](url)", link.MarkdownAfter);
    }

    [Fact]
    public void CodeBlock_HasNewLineBeforeAndMultilineWrap()
    {
        var cb = ToolbarItems.CodeBlock;
        Assert.StartsWith("```", cb.MarkdownBefore);
        Assert.EndsWith("```", cb.MarkdownAfter);
        Assert.True(cb.NewLineBefore);
    }

    [Fact]
    public void AllDefaultItems_HaveNonEmptyId()
    {
        foreach (var item in ToolbarItems.Default)
        {
            Assert.False(string.IsNullOrEmpty(item.Id),
                $"ToolbarItem should have a non-empty Id");
        }
    }

    [Fact]
    public void AllDefaultItems_HaveTooltipKeyOrAreSeparators()
    {
        foreach (var item in ToolbarItems.Default)
        {
            if (!item.IsSeparator)
            {
                Assert.False(string.IsNullOrEmpty(item.TooltipKey),
                    $"ToolbarItem '{item.Id}' should have a TooltipKey");
            }
        }
    }
}
