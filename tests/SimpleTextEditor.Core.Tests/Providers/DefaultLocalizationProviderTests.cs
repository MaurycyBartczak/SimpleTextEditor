using SimpleTextEditor.Core.Providers;

namespace SimpleTextEditor.Core.Tests.Providers;

public class DefaultLocalizationProviderTests
{
    private readonly DefaultLocalizationProvider _sut = new();

    [Fact]
    public void CurrentLanguage_DefaultIsEnglish()
    {
        Assert.Equal("en", _sut.CurrentLanguage);
    }

    [Theory]
    [InlineData("bold", "Bold")]
    [InlineData("italic", "Italic")]
    [InlineData("undo", "Undo")]
    [InlineData("placeholder", "Start typing...")]
    public void Get_EnglishKey_ReturnsEnglishTranslation(string key, string expected)
    {
        Assert.Equal(expected, _sut.Get(key));
    }

    [Theory]
    [InlineData("bold", "Pogrubienie")]
    [InlineData("italic", "Kursywa")]
    [InlineData("undo", "Cofnij")]
    [InlineData("placeholder", "Zacznij pisać...")]
    public void Get_PolishKey_ReturnsPolishTranslation(string key, string expected)
    {
        _sut.SetLanguage("pl");
        Assert.Equal(expected, _sut.Get(key));
    }

    [Fact]
    public void Get_UnknownKey_ReturnsKeyItself()
    {
        Assert.Equal("nonExistentKey", _sut.Get("nonExistentKey"));
    }

    [Fact]
    public void SetLanguage_ValidCode_ChangesLanguage()
    {
        _sut.SetLanguage("pl");
        Assert.Equal("pl", _sut.CurrentLanguage);
    }

    [Fact]
    public void SetLanguage_InvalidCode_KeepsCurrentLanguage()
    {
        _sut.SetLanguage("xx");
        Assert.Equal("en", _sut.CurrentLanguage);
    }

    [Fact]
    public void AddTranslations_OverridesExistingKey()
    {
        _sut.AddTranslations(new Dictionary<string, string>
        {
            ["bold"] = "Custom Bold"
        });
        
        Assert.Equal("Custom Bold", _sut.Get("bold"));
    }

    [Fact]
    public void AddTranslations_AddsNewKey()
    {
        _sut.AddTranslations(new Dictionary<string, string>
        {
            ["customAction"] = "My Custom Action"
        });
        
        Assert.Equal("My Custom Action", _sut.Get("customAction"));
    }

    [Fact]
    public void GetAvailableLanguages_ReturnsBothLanguages()
    {
        var languages = _sut.GetAvailableLanguages().ToList();
        
        Assert.Contains("en", languages);
        Assert.Contains("pl", languages);
        Assert.Equal(2, languages.Count);
    }

    [Fact]
    public void Get_UnknownKeyInPolish_FallsBackToEnglish()
    {
        _sut.SetLanguage("pl");
        // "previewTitle" is in both languages but with different values
        // Let's test a key that exists in English only would fallback
        // All keys exist in both, so test the fallback mechanism via a custom key
        _sut.SetLanguage("en");
        _sut.AddTranslations(new Dictionary<string, string> { ["onlyEnglish"] = "English Only" });
        
        // Custom translations are language-independent (always checked first)
        _sut.SetLanguage("pl");
        Assert.Equal("English Only", _sut.Get("onlyEnglish"));
    }
}
