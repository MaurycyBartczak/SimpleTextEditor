namespace SimpleTextEditor.Blazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SimpleTextEditor.Blazor.Services;
using SimpleTextEditor.Core.Abstractions;
using SimpleTextEditor.Core.Models;
using SimpleTextEditor.Core.Providers;
using SimpleTextEditor.Core.Services;
using SimpleTextEditor.Core.Themes;

/// <summary>
/// Bazowy komponent edytora Markdown z paskiem narzędzi, polem edycji i panelem podglądu.
/// Zapewnia czyste edytowanie Markdown bez trybu WYSIWYG.
/// </summary>
public partial class EditorBase : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Środowisko uruchomieniowe JavaScript do operacji interop.
    /// </summary>
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    
    private ElementReference _containerRef;
    private ElementReference _textareaRef;
    private EditorJsInterop? _jsInterop;
    private string _internalValue = "";
    private bool _isFullscreen = false;
    private bool _showPreview = true;
    
    /// <summary>
    /// Wartość treści Markdown.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = "";
    
    /// <summary>
    /// Callback wywoływany przy zmianie wartości.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }
    
    /// <summary>
    /// Konfiguracja trybu podglądu.
    /// </summary>
    [Parameter]
    public PreviewMode PreviewMode { get; set; } = PreviewMode.SideBySide;
    
    /// <summary>
    /// Nazwa motywu ("light" lub "dark").
    /// </summary>
    [Parameter]
    public string Theme { get; set; } = "light";
    
    /// <summary>
    /// Tekst zastępczy wyświetlany gdy edytor jest pusty.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }
    
    /// <summary>
    /// Niestandardowa kolekcja elementów paska narzędzi.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ToolbarItem>? ToolbarItems { get; set; }
    
    /// <summary>
    /// Niestandardowy dostawca ikon dla paska narzędzi.
    /// </summary>
    [Parameter]
    public IIconProvider? IconProvider { get; set; }
    
    /// <summary>
    /// Niestandardowy dostawca lokalizacji dla tekstów interfejsu.
    /// </summary>
    [Parameter]
    public ILocalizationProvider? LocalizationProvider { get; set; }
    
    /// <summary>
    /// Niestandardowy parser Markdown do konwersji treści.
    /// </summary>
    [Parameter]
    public IMarkdownParser? MarkdownParser { get; set; }
    
    /// <summary>
    /// Niestandardowa instancja motywu do stylizacji.
    /// </summary>
    [Parameter]
    public IEditorTheme? EditorTheme { get; set; }
    
    /// <summary>
    /// Minimalna wysokość edytora w pikselach.
    /// </summary>
    [Parameter]
    public int MinHeight { get; set; } = 300;
    
    /// <summary>
    /// Maksymalna wysokość edytora w pikselach (0 = brak limitu).
    /// </summary>
    [Parameter]
    public int MaxHeight { get; set; } = 0;
    
    /// <summary>
    /// Tryb tylko do odczytu uniemożliwia edycję.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; } = false;
    
    /// <summary>
    /// Dodatkowa klasa CSS dla kontenera.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }
    
    /// <summary>
    /// Callback wywoływany przy zmianie treści.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnChange { get; set; }
    
    // Rozwiązane instancje z wartościami domyślnymi
    private IIconProvider IconProviderInstance => IconProvider ?? new MaterialIconProvider();
    private ILocalizationProvider LocalizationProviderInstance => LocalizationProvider ?? new DefaultLocalizationProvider();
    private IMarkdownParser MarkdownParserInstance => MarkdownParser ?? new MarkdownService();
    private IEditorTheme ThemeInstance => EditorTheme ?? (Theme == "dark" ? new DarkTheme() : new LightTheme());
    private IReadOnlyList<ToolbarItem> ToolbarItemsList => ToolbarItems ?? Core.Models.ToolbarItems.Default;
    
    private PreviewMode CurrentPreviewMode => PreviewMode;
    
    private bool ShowPreview
    {
        get => _showPreview;
        set => _showPreview = value;
    }
    
    private string InternalValue
    {
        get => _internalValue;
        set
        {
            if (_internalValue != value)
            {
                _internalValue = value;
                _ = NotifyValueChanged();
            }
        }
    }
    
    private string PreviewHtml => MarkdownParserInstance.ToHtml(_internalValue);
    private string EffectivePlaceholder => Placeholder ?? LocalizationProviderInstance.Get("placeholder");
    private string ContainerClass => $"{ThemeInstance.ContainerClass} {CssClass} {(_isFullscreen ? "ste-fullscreen" : "")}".Trim();
    private string LayoutClass => CurrentPreviewMode switch
    {
        PreviewMode.SideBySide => "ste-side-by-side",
        PreviewMode.Toggle => "ste-toggle",
        _ => ""
    };
    
    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _internalValue = Value;
        _jsInterop = new EditorJsInterop(JSRuntime);
    }
    
    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Value != _internalValue)
        {
            _internalValue = Value;
        }
    }
    
    private async Task NotifyValueChanged()
    {
        await ValueChanged.InvokeAsync(_internalValue);
        await OnChange.InvokeAsync(_internalValue);
    }
    
    private async Task HandleToolbarClick(ToolbarItem item)
    {
        if (ReadOnly) return;
        
        switch (item.Id)
        {
            case "undo":
                // Obsługiwane przez przeglądarkę
                break;
            case "redo":
                // Obsługiwane przez przeglądarkę
                break;
            case "preview":
                ShowPreview = !ShowPreview;
                break;
            case "fullscreen":
                await ToggleFullscreen();
                break;
            default:
                await InsertMarkdown(item);
                break;
        }
        
        StateHasChanged();
    }
    
    private async Task InsertMarkdown(ToolbarItem item)
    {
        if (_jsInterop == null) return;
        
        var newValue = await _jsInterop.InsertTextAsync(
            _textareaRef,
            item.MarkdownBefore,
            item.MarkdownAfter,
            item.NewLineBefore);
        
        _internalValue = newValue;
        await NotifyValueChanged();
    }
    
    private async Task ToggleFullscreen()
    {
        _isFullscreen = !_isFullscreen;
        if (_jsInterop != null)
        {
            await _jsInterop.ToggleFullscreenAsync(_containerRef, _isFullscreen);
        }
    }
    
    private bool IsToolbarItemActive(string itemId)
    {
        return itemId switch
        {
            "preview" => ShowPreview,
            "fullscreen" => _isFullscreen,
            _ => false
        };
    }
    
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        // Obsługa skrótów klawiaturowych
        if (e.CtrlKey)
        {
            var item = e.Key.ToLower() switch
            {
                "b" => Core.Models.ToolbarItems.Bold,
                "i" => Core.Models.ToolbarItems.Italic,
                _ => null
            };
            
            if (item != null)
            {
                await InsertMarkdown(item);
            }
        }
    }
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_jsInterop != null)
        {
            await _jsInterop.DisposeAsync();
        }
    }
}
