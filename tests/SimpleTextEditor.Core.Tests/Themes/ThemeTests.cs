using SimpleTextEditor.Core.Themes;

namespace SimpleTextEditor.Core.Tests.Themes;

public class ThemeTests
{
    [Fact]
    public void LightTheme_HasCorrectName()
    {
        var theme = new LightTheme();
        Assert.Equal("light", theme.Name);
    }

    [Fact]
    public void LightTheme_ContainerClassContainsLight()
    {
        var theme = new LightTheme();
        Assert.Contains("ste-light", theme.ContainerClass);
        Assert.Contains("ste-container", theme.ContainerClass);
    }

    [Fact]
    public void LightTheme_ToolbarClassContainsLight()
    {
        var theme = new LightTheme();
        Assert.Contains("ste-toolbar-light", theme.ToolbarClass);
    }

    [Fact]
    public void LightTheme_EditorClassContainsLight()
    {
        var theme = new LightTheme();
        Assert.Contains("ste-editor-light", theme.EditorClass);
    }

    [Fact]
    public void LightTheme_PreviewClassContainsLight()
    {
        var theme = new LightTheme();
        Assert.Contains("ste-preview-light", theme.PreviewClass);
    }

    [Fact]
    public void LightTheme_AdditionalStylesIsEmpty()
    {
        var theme = new LightTheme();
        Assert.Equal(string.Empty, theme.AdditionalStyles);
    }

    [Fact]
    public void DarkTheme_HasCorrectName()
    {
        var theme = new DarkTheme();
        Assert.Equal("dark", theme.Name);
    }

    [Fact]
    public void DarkTheme_ContainerClassContainsDark()
    {
        var theme = new DarkTheme();
        Assert.Contains("ste-dark", theme.ContainerClass);
        Assert.Contains("ste-container", theme.ContainerClass);
    }

    [Fact]
    public void DarkTheme_ToolbarClassContainsDark()
    {
        var theme = new DarkTheme();
        Assert.Contains("ste-toolbar-dark", theme.ToolbarClass);
    }

    [Fact]
    public void DarkTheme_EditorClassContainsDark()
    {
        var theme = new DarkTheme();
        Assert.Contains("ste-editor-dark", theme.EditorClass);
    }

    [Fact]
    public void DarkTheme_PreviewClassContainsDark()
    {
        var theme = new DarkTheme();
        Assert.Contains("ste-preview-dark", theme.PreviewClass);
    }

    [Fact]
    public void DarkTheme_AdditionalStylesIsEmpty()
    {
        var theme = new DarkTheme();
        Assert.Equal(string.Empty, theme.AdditionalStyles);
    }
}
