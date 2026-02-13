namespace SimpleTextEditor.Radzen.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SimpleTextEditor.Blazor.Components;
using SimpleTextEditor.Blazor.Services;
using SimpleTextEditor.Core.Abstractions;
using SimpleTextEditor.Core.Models;
using SimpleTextEditor.Core.Providers;
using SimpleTextEditor.Core.Services;
using SimpleTextEditor.Core.Themes;

/// <summary>
/// Komponent edytora Markdown w stylu Radzen z trybami WYSIWYG i Markdown.
/// Zapewnia edycję tekstu sformatowanego z komponentami UI Radzen.
/// </summary>
public partial class RadzenMarkdownEditor : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Środowisko uruchomieniowe JavaScript do operacji interop.
    /// </summary>
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    
    private ElementReference _textareaRef;
    private ElementReference _wysiwygRef;
    private InputFile? _imageInputRef;
    private EditorJsInterop? _jsInterop;
    private WysiwygJsInterop? _wysiwygJsInterop;
    private string _internalValue = "";
    private bool _isFullscreen = false;
    private bool _showPreview = true;
    private EditorMode _currentMode = EditorMode.Wysiwyg;
    private bool _isInitialized = false;
    private readonly string _imageInputId = $"ste-image-input-{Guid.NewGuid():N}";
    private CancellationTokenSource? _debounceCts;
    private const int DebounceDelayMs = 300;
    
    /// <summary>
    /// Wartość treści Markdown.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = "";
    
    /// <summary>
    /// Callback wywoływany przy zmianie wartości. Używany do dwukierunkowego wiązania.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }
    
    /// <summary>
    /// Konfiguracja trybu podglądu dla trybu Markdown.
    /// </summary>
    [Parameter]
    public PreviewMode PreviewMode { get; set; } = PreviewMode.SideBySide;
    
    /// <summary>
    /// Początkowy tryb edytora (Wysiwyg lub Markdown).
    /// </summary>
    [Parameter]
    public EditorMode Mode { get; set; } = EditorMode.Wysiwyg;
    
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
    
    /// <summary>
    /// Niestandardowy handler uploadu obrazów. Jeśli nie ustawiony, obrazy są konwertowane na Base64 data URI.
    /// Zaimplementuj IImageUploadHandler aby dostosować przechowywanie obrazów.
    /// </summary>
    [Parameter]
    public IImageUploadHandler? ImageUploadHandler { get; set; }
    
    // Rozwiązane instancje z wartościami domyślnymi
    private IImageUploadHandler ImageUploadHandlerInstance => ImageUploadHandler ?? new Base64ImageUploadHandler();
    private IIconProvider IconProviderInstance => IconProvider ?? new MaterialIconProvider();
    private ILocalizationProvider LocalizationProviderInstance => LocalizationProvider ?? new DefaultLocalizationProvider();
    private IMarkdownParser MarkdownParserInstance => MarkdownParser ?? new MarkdownService();
    private IEditorTheme ThemeInstance => EditorTheme ?? (Theme == "dark" ? new DarkTheme() : new LightTheme());
    private IReadOnlyList<ToolbarItem> ToolbarItemsList => ToolbarItems ?? Core.Models.ToolbarItems.Default;
    private IHtmlToMarkdownConverter HtmlToMarkdownConverter => new HtmlToMarkdownConverter();
    
    private EditorMode CurrentMode => _currentMode;
    
    // Czy wyświetlać panel podglądu (na podstawie trybu i stanu przełącznika)
    private bool ShouldShowPreviewPanel => PreviewMode != PreviewMode.None && 
        (PreviewMode == PreviewMode.Toggle || _showPreview);
    
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
    private string ContainerClass => $"ste-radzen-container {ThemeInstance.ContainerClass} {CssClass} {(_isFullscreen ? "ste-fullscreen" : "")}".Trim();
    private string LayoutClass => CurrentMode == EditorMode.Markdown 
        ? PreviewMode switch
        {
            PreviewMode.SideBySide when _showPreview => "ste-side-by-side",
            PreviewMode.Toggle => "ste-toggle",
            _ => ""
        }
        : "";
    
    private string EditorStyle => $"min-height: {MinHeight}px; {(MaxHeight > 0 ? $"max-height: {MaxHeight}px;" : "")}";
    
    private string GetToolbarClass() => ThemeInstance.ToolbarClass;
    
    // Ukryj edytor gdy w trybie przełączania i wyświetlany jest podgląd
    private string GetEditorHiddenClass() => 
        PreviewMode == PreviewMode.Toggle && _showPreview ? "ste-hidden" : "";
    
    // Ukryj podgląd gdy w trybie przełączania i podgląd nie jest wyświetlany
    private string GetPreviewHiddenClass() => 
        PreviewMode == PreviewMode.Toggle && !_showPreview ? "ste-hidden" : "";
    
    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _internalValue = Value;
        _currentMode = Mode;
        _jsInterop = new EditorJsInterop(JSRuntime);
        _wysiwygJsInterop = new WysiwygJsInterop(JSRuntime);
    }
    
    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isInitialized = true;
            if (_currentMode == EditorMode.Wysiwyg && _wysiwygJsInterop != null)
            {
                // Ustawienie początkowej treści HTML w trybie WYSIWYG
                var html = MarkdownParserInstance.ToHtml(_internalValue);
                await _wysiwygJsInterop.SetHtmlAsync(_wysiwygRef, html);
            }
        }
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
    
    private Task HandleWysiwygInput()
    {
        // Nie synchronizuj HTML do Markdown przy każdym naciśnięciu klawisza — powoduje problemy z SignalR przy dużej treści
        // Treść zostanie zsynchronizowana podczas przełączania trybów lub na żądanie
        return Task.CompletedTask;
    }
    
    private async Task HandleToolbarClick(ToolbarItem item)
    {
        if (ReadOnly) return;
        
        switch (item.Id)
        {
            case "switchMode":
                await SwitchEditorMode();
                break;
            case "undo":
                await ExecuteUndo();
                break;
            case "redo":
                await ExecuteRedo();
                break;
            case "preview":
                _showPreview = !_showPreview;
                break;
            case "fullscreen":
                _isFullscreen = !_isFullscreen;
                break;
            case "image":
                await TriggerImageUpload();
                break;
            default:
                if (_currentMode == EditorMode.Wysiwyg)
                    await ExecuteWysiwygCommand(item);
                else
                    await InsertMarkdown(item);
                break;
        }
        
        StateHasChanged();
    }
    
    private async Task SwitchEditorMode()
    {
        try
        {
            if (_currentMode == EditorMode.Markdown)
            {
                // Przełączenie na WYSIWYG
                _currentMode = EditorMode.Wysiwyg;
                StateHasChanged();
                await Task.Delay(50); // Oczekiwanie na renderowanie
                
                if (_wysiwygJsInterop != null)
                {
                    var html = MarkdownParserInstance.ToHtml(_internalValue);
                    await _wysiwygJsInterop.SetHtmlAsync(_wysiwygRef, html);
                }
            }
            else
            {
                // Przełączenie na Markdown
                if (_wysiwygJsInterop != null)
                {
                    var html = await _wysiwygJsInterop.GetHtmlAsync(_wysiwygRef);
                    _internalValue = HtmlToMarkdownConverter.Convert(html);
                    await NotifyValueChanged();
                }
                _currentMode = EditorMode.Markdown;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd przełączania trybu: {ex.Message}");
            // Awaryjne przełączenie trybu bez konwersji
            _currentMode = _currentMode == EditorMode.Markdown ? EditorMode.Wysiwyg : EditorMode.Markdown;
        }
    }
    
    private async Task ExecuteWysiwygCommand(ToolbarItem item)
    {
        if (_wysiwygJsInterop == null) return;
        
        try
        {
            switch (item.Id)
            {
                case "bold":
                    await _wysiwygJsInterop.ExecCommandAsync("bold");
                    break;
                case "italic":
                    await _wysiwygJsInterop.ExecCommandAsync("italic");
                    break;
                case "strikethrough":
                    await _wysiwygJsInterop.ExecCommandAsync("strikeThrough");
                    break;
                case "heading1":
                    await _wysiwygJsInterop.FormatBlockAsync("h1");
                    break;
                case "heading2":
                    await _wysiwygJsInterop.FormatBlockAsync("h2");
                    break;
                case "heading3":
                    await _wysiwygJsInterop.FormatBlockAsync("h3");
                    break;
                case "bulletList":
                    await _wysiwygJsInterop.InsertUnorderedListAsync();
                    break;
                case "numberedList":
                    await _wysiwygJsInterop.InsertOrderedListAsync();
                    break;
                case "quote":
                    await _wysiwygJsInterop.IndentAsync();
                    break;
                case "alignLeft":
                    await _wysiwygJsInterop.AlignTextAsync("left");
                    break;
                case "alignCenter":
                    await _wysiwygJsInterop.AlignTextAsync("center");
                    break;
                case "alignRight":
                    await _wysiwygJsInterop.AlignTextAsync("right");
                    break;
                case "link":
                    await _wysiwygJsInterop.CreateLinkAsync("https://");
                    break;
                case "image":
                    await _wysiwygJsInterop.InsertImageAsync("https://via.placeholder.com/150");
                    break;
                case "code":
                    await _wysiwygJsInterop.InsertHtmlAsync("<code>code</code>");
                    break;
                case "codeBlock":
                    await _wysiwygJsInterop.InsertHtmlAsync("<pre><code>// code here</code></pre>");
                    break;
                case "table":
                    await _wysiwygJsInterop.InsertHtmlAsync(
                        @"<table>
                            <thead>
                                <tr><th>Header 1</th><th>Header 2</th><th>Header 3</th></tr>
                            </thead>
                            <tbody>
                                <tr><td>Cell 1</td><td>Cell 2</td><td>Cell 3</td></tr>
                                <tr><td>Cell 4</td><td>Cell 5</td><td>Cell 6</td></tr>
                            </tbody>
                        </table>");
                    break;
                case "horizontalRule":
                    await _wysiwygJsInterop.InsertHorizontalRuleAsync();
                    break;
                default:
                    break;
            }
            
            // Zaktualizuj wartość Markdown po wykonaniu polecenia
            await HandleWysiwygInput();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd wykonania polecenia WYSIWYG: {ex.Message}");
        }
    }
    
    private async Task ExecuteUndo()
    {
        await JSRuntime.InvokeVoidAsync("eval", "document.execCommand('undo')");
    }
    
    private async Task ExecuteRedo()
    {
        await JSRuntime.InvokeVoidAsync("eval", "document.execCommand('redo')");
    }
    
    private async Task InsertMarkdown(ToolbarItem item)
    {
        if (_jsInterop == null) return;
        
        try
        {
            var newValue = await _jsInterop.InsertTextAsync(
                _textareaRef,
                item.MarkdownBefore,
                item.MarkdownAfter,
                item.NewLineBefore);
            
            _internalValue = newValue;
            await NotifyValueChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd wstawiania Markdown: {ex.Message}");
        }
    }
    
    private bool IsToolbarItemActive(string itemId)
    {
        return itemId switch
        {
            "switchMode" => _currentMode == EditorMode.Markdown,
            "preview" => _showPreview,
            "fullscreen" => _isFullscreen,
            _ => false
        };
    }
    
    private async Task TriggerImageUpload()
    {
        await JSRuntime.InvokeVoidAsync("eval", 
            $"document.getElementById('{_imageInputId}')?.click()");
    }
    
    private async Task HandleImageFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;
        
        var handler = ImageUploadHandlerInstance;
        
        // Walidacja typu zawartości
        if (!handler.AllowedContentTypes.Contains(file.ContentType))
        {
            Console.WriteLine($"Nieobsługiwany typ pliku: {file.ContentType}");
            return;
        }
        
        // Walidacja rozmiaru pliku
        var maxSize = handler.MaxFileSizeBytes > 0 ? handler.MaxFileSizeBytes : 10 * 1024 * 1024;
        if (file.Size > maxSize)
        {
            Console.WriteLine($"Plik za duży: {file.Size} bajtów");
            return;
        }
        
        try
        {
            // Odczyt zawartości pliku
            using var stream = file.OpenReadStream(maxAllowedSize: maxSize);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var content = ms.ToArray();
            
            // Upload i pobranie URL
            var url = await handler.UploadAsync(file.Name, content, file.ContentType);
            
            // Wstawienie obrazu do edytora
            await InsertImageUrl(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd uploadu obrazu: {ex.Message}");
        }
    }
    
    private async Task InsertImageUrl(string url)
    {
        if (_currentMode == EditorMode.Wysiwyg)
        {
            if (_wysiwygJsInterop != null)
            {
                await _wysiwygJsInterop.InsertImageAsync(url);
                await HandleWysiwygInput();
            }
        }
        else
        {
            // Tryb Markdown — wstawianie składni obrazu Markdown
            if (_jsInterop != null)
            {
                var newValue = await _jsInterop.InsertTextAsync(
                    _textareaRef,
                    "![image](",
                    ")",
                    false);
                
                // Wstawienie URL w środku
                _internalValue = newValue.Replace("![image]()", $"![image]({url})");
                await NotifyValueChanged();
            }
        }
    }
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        
        if (_jsInterop != null)
        {
            try
            {
                await _jsInterop.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignoruj — obwód już rozłączony
            }
        }
        
        if (_wysiwygJsInterop != null)
        {
            try
            {
                await _wysiwygJsInterop.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignoruj — obwód już rozłączony
            }
        }
    }
}
