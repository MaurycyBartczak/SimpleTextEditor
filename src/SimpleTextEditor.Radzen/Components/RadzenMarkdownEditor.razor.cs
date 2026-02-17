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
    private SteJsInterop? _jsInterop;
    private string _internalValue = "";
    private bool _isFullscreen = false;
    private bool _showPreview = true;
    private EditorMode _currentMode = EditorMode.Wysiwyg;
    private bool _isInitialized = false;
    private readonly string _imageInputId = $"ste-image-input-{Guid.NewGuid():N}";
    private CancellationTokenSource? _debounceCts;
    private const int DebounceDelayMs = 300;
    private DotNetObjectReference<RadzenMarkdownEditor>? _dotNetRef;
    
    // Stan popupu resize obrazka
    private bool _showResizePopup = false;
    private int _resizePopupWidth = 0;
    private int _resizePopupHeight = 0;
    
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
    
    /// <summary>
    /// Niestandardowy konwerter HTML do Markdown. Jeśli null, używa domyślnego.
    /// </summary>
    [Parameter]
    public IHtmlToMarkdownConverter? HtmlToMarkdownConverter { get; set; }
    
    // Rozwiązane instancje z wartościami domyślnymi
    private IImageUploadHandler ImageUploadHandlerInstance => ImageUploadHandler ?? new Base64ImageUploadHandler();
    private IIconProvider IconProviderInstance => IconProvider ?? new MaterialIconProvider();
    private ILocalizationProvider LocalizationProviderInstance => LocalizationProvider ?? new DefaultLocalizationProvider();
    private IMarkdownParser MarkdownParserInstance => MarkdownParser ?? new MarkdownService();
    private IEditorTheme ThemeInstance => EditorTheme ?? (Theme == "dark" ? new DarkTheme() : new LightTheme());
    private IReadOnlyList<ToolbarItem> ToolbarItemsList => ToolbarItems ?? Core.Models.ToolbarItems.Default;
    private IHtmlToMarkdownConverter HtmlToMarkdownConverterInstance => HtmlToMarkdownConverter ?? new HtmlToMarkdownConverter();
    
    private EditorMode CurrentMode => _currentMode;
    
    // Czy wyświetlać panel podglądu
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
    
    private int WordCount => string.IsNullOrWhiteSpace(_internalValue) 
        ? 0 
        : _internalValue.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    
    private int CharacterCount => _internalValue.Length;
    
    private int LineCount => string.IsNullOrEmpty(_internalValue) 
        ? 1 
        : _internalValue.Split('\n').Length;
    
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
        _jsInterop = new SteJsInterop(JSRuntime);
    }
    
    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isInitialized = true;
            if (_currentMode == EditorMode.Wysiwyg && _jsInterop != null)
            {
                // Ustawienie początkowej treści HTML w trybie WYSIWYG
                var html = MarkdownParserInstance.ToHtml(_internalValue);
                await _jsInterop.SetHtmlAsync(_wysiwygRef, html);
                
                // Inicjalizacja modułu zmiany rozmiaru obrazów
                await InitImageResize();
                
                // Inicjalizacja skrótów klawiaturowych
                await _jsInterop.InitKeyboardShortcutsAsync(_wysiwygRef);
                
                // Inicjalizacja drag & drop / paste obrazków
                await _jsInterop.InitImageDragDropAsync(_wysiwygRef, _dotNetRef!);
            }
        }
    }
    
    private async Task InitImageResize()
    {
        if (_jsInterop == null) return;
        _dotNetRef?.Dispose();
        _dotNetRef = DotNetObjectReference.Create(this);
        await _jsInterop.InitImageResizeAsync(_wysiwygRef, _dotNetRef);
    }
    
    /// <summary>
    /// Callback wywoływany przez JS po zmianie rozmiaru obrazu (drag).
    /// </summary>
    [JSInvokable]
    public async Task OnImageResized()
    {
        if (_jsInterop == null) return;
        
        var html = await _jsInterop.GetHtmlAsync(_wysiwygRef);
        _internalValue = HtmlToMarkdownConverterInstance.Convert(html);
        await NotifyValueChanged();
        StateHasChanged();
    }
    
    /// <summary>
    /// Callback wywoływany przez JS po podwójnym kliknięciu na obrazek — otwiera popup Blazor.
    /// </summary>
    [JSInvokable]
    public void OnImageDblClick(int width, int height)
    {
        _resizePopupWidth = width;
        _resizePopupHeight = height;
        _showResizePopup = true;
        StateHasChanged();
    }
    
    /// <summary>
    /// Callback wywoływany przez JS po odznaczeniu obrazka (Escape).
    /// </summary>
    [JSInvokable]
    public void OnImageDeselected()
    {
        _showResizePopup = false;
        StateHasChanged();
    }
    
    /// <summary>
    /// Obsługa zastosowania rozmiaru z popupu Blazor.
    /// </summary>
    private async Task HandleResizeApply((int Width, int Height) size)
    {
        if (_jsInterop == null) return;
        await _jsInterop.SetSelectedImageSizeAsync(size.Width, size.Height);
        _showResizePopup = false;
        
        // Zsynchronizuj HTML → Markdown
        var html = await _jsInterop.GetHtmlAsync(_wysiwygRef);
        _internalValue = HtmlToMarkdownConverterInstance.Convert(html);
        await NotifyValueChanged();
    }
    
    /// <summary>
    /// Obsługa zamknięcia popupu resize.
    /// </summary>
    private void HandleResizeClose()
    {
        _showResizePopup = false;
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
    
    private async Task HandleWysiwygInput()
    {
        // Debounce: poczekaj DebounceDelayMs, potem zsynchronizuj HTML → Markdown
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;
        
        try
        {
            await Task.Delay(DebounceDelayMs, token);
            if (token.IsCancellationRequested || _jsInterop == null) return;
            
            var html = await _jsInterop.GetHtmlAsync(_wysiwygRef);
            _internalValue = HtmlToMarkdownConverterInstance.Convert(html);
            await NotifyValueChanged();
            StateHasChanged();
        }
        catch (TaskCanceledException)
        {
            // Ignoruj — nowy input anulował poprzedni debounce
        }
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
                _showResizePopup = false;
                StateHasChanged();
                await Task.Delay(50);
                
                if (_jsInterop != null)
                {
                    var html = MarkdownParserInstance.ToHtml(_internalValue);
                    await _jsInterop.SetHtmlAsync(_wysiwygRef, html);
                    
                    // Reinicjalizacja modułów JS
                    await _jsInterop.DisposeImageResizeAsync();
                    await _jsInterop.DisposeKeyboardShortcutsAsync();
                    await _jsInterop.DisposeImageDragDropAsync();
                    await InitImageResize();
                    await _jsInterop.InitKeyboardShortcutsAsync(_wysiwygRef);
                    await _jsInterop.InitImageDragDropAsync(_wysiwygRef, _dotNetRef!);
                }
            }
            else
            {
                // Przełączenie na Markdown — zwolnij moduł resize
                _showResizePopup = false;
                if (_jsInterop != null)
                {
                    await _jsInterop.DisposeImageResizeAsync();
                    await _jsInterop.DisposeKeyboardShortcutsAsync();
                    await _jsInterop.DisposeImageDragDropAsync();
                    var html = await _jsInterop.GetHtmlAsync(_wysiwygRef);
                    _internalValue = HtmlToMarkdownConverterInstance.Convert(html);
                    await NotifyValueChanged();
                }
                _currentMode = EditorMode.Markdown;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd przełączania trybu: {ex.Message}");
            _currentMode = _currentMode == EditorMode.Markdown ? EditorMode.Wysiwyg : EditorMode.Markdown;
        }
    }
    
    private async Task ExecuteWysiwygCommand(ToolbarItem item)
    {
        if (_jsInterop == null) return;
        
        try
        {
            switch (item.Id)
            {
                case "bold":
                    await _jsInterop.ExecCommandAsync("bold");
                    break;
                case "italic":
                    await _jsInterop.ExecCommandAsync("italic");
                    break;
                case "strikethrough":
                    await _jsInterop.ExecCommandAsync("strikeThrough");
                    break;
                case "heading1":
                    await _jsInterop.FormatBlockAsync("h1");
                    break;
                case "heading2":
                    await _jsInterop.FormatBlockAsync("h2");
                    break;
                case "heading3":
                    await _jsInterop.FormatBlockAsync("h3");
                    break;
                case "bulletList":
                    await _jsInterop.InsertUnorderedListAsync();
                    break;
                case "numberedList":
                    await _jsInterop.InsertOrderedListAsync();
                    break;
                case "quote":
                    await _jsInterop.IndentAsync();
                    break;
                case "alignLeft":
                    await _jsInterop.AlignTextAsync("left");
                    break;
                case "alignCenter":
                    await _jsInterop.AlignTextAsync("center");
                    break;
                case "alignRight":
                    await _jsInterop.AlignTextAsync("right");
                    break;
                case "link":
                    await _jsInterop.CreateLinkAsync("https://");
                    break;
                case "image":
                    await _jsInterop.InsertImageAsync("https://via.placeholder.com/150");
                    break;
                case "code":
                    await _jsInterop.InsertHtmlAsync("<code>code</code>");
                    break;
                case "codeBlock":
                    await _jsInterop.InsertHtmlAsync("<pre><code>// code here</code></pre>");
                    break;
                case "table":
                    await _jsInterop.InsertHtmlAsync(
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
                    await _jsInterop.InsertHorizontalRuleAsync();
                    break;
                default:
                    break;
            }
            
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
        
        if (!handler.AllowedContentTypes.Contains(file.ContentType))
        {
            Console.WriteLine($"Nieobsługiwany typ pliku: {file.ContentType}");
            return;
        }
        
        var maxSize = handler.MaxFileSizeBytes > 0 ? handler.MaxFileSizeBytes : 10 * 1024 * 1024;
        if (file.Size > maxSize)
        {
            Console.WriteLine($"Plik za duży: {file.Size} bajtów");
            return;
        }
        
        try
        {
            using var stream = file.OpenReadStream(maxAllowedSize: maxSize);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var content = ms.ToArray();
            
            var url = await handler.UploadAsync(file.Name, content, file.ContentType);
            await InsertImageUrl(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd uploadu obrazu: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Callback wywoływany przez JS po upuszczeniu lub wklejeniu obrazka.
    /// </summary>
    [JSInvokable]
    public async Task OnImageDropped(string fileName, string base64, string contentType)
    {
        var handler = ImageUploadHandlerInstance;
        
        if (!handler.AllowedContentTypes.Contains(contentType))
        {
            return;
        }
        
        try
        {
            var content = Convert.FromBase64String(base64);
            
            var maxSize = handler.MaxFileSizeBytes > 0 ? handler.MaxFileSizeBytes : 10 * 1024 * 1024;
            if (content.Length > maxSize)
            {
                return;
            }
            
            var url = await handler.UploadAsync(fileName, content, contentType);
            await InsertImageUrl(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd uploadu obrazu (drag/paste): {ex.Message}");
        }
    }
    
    private async Task InsertImageUrl(string url)
    {
        if (_currentMode == EditorMode.Wysiwyg)
        {
            if (_jsInterop != null)
            {
                await _jsInterop.InsertImageAsync(url);
                await HandleWysiwygInput();
            }
        }
        else
        {
            if (_jsInterop != null)
            {
                var newValue = await _jsInterop.InsertTextAsync(
                    _textareaRef,
                    "![image](",
                    ")",
                    false);
                
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
        _dotNetRef?.Dispose();
        
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
    }
}
