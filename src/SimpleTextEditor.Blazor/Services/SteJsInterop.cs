using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SimpleTextEditor.Blazor.Services;

/// <summary>
/// Zunifikowany wrapper C# dla wszystkich operacji JavaScript interop.
/// Zastępuje EditorJsInterop + WysiwygJsInterop + osobny moduł resize.
/// </summary>
public class SteJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private bool _isDisposed;

    public SteJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SimpleTextEditor.Blazor/js/ste-interop.js");
        }
        return _module;
    }

    // ============================================================
    // Sekcja 1: Operacje na textarea (tryb Markdown)
    // ============================================================

    /// <summary>
    /// Wstawia tekst w bieżącej pozycji kursora textarea.
    /// </summary>
    public async ValueTask<string> InsertTextAsync(ElementReference textarea, string before, string after, bool newLineBefore)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("insertText", textarea, before, after, newLineBefore);
    }

    /// <summary>
    /// Pobiera numer bieżącej linii w textarea.
    /// </summary>
    public async ValueTask<int> GetCurrentLineAsync(ElementReference textarea)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<int>("getCurrentLine", textarea);
    }

    /// <summary>
    /// Synchronizuje scroll edytora z podglądem.
    /// </summary>
    public async ValueTask SyncScrollAsync(ElementReference editor, ElementReference preview)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("syncScroll", editor, preview);
    }

    // ============================================================
    // Sekcja 2: Operacje WYSIWYG (contenteditable)
    // ============================================================

    /// <summary>
    /// Wykonuje polecenie formatowania na contenteditable.
    /// </summary>
    public async ValueTask ExecCommandAsync(string command, string? value = null)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("execCommand", command, value);
    }

    /// <summary>
    /// Pobiera HTML z elementu contenteditable.
    /// </summary>
    public async ValueTask<string> GetHtmlAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("getHtml", element);
    }

    /// <summary>
    /// Ustawia HTML elementu contenteditable.
    /// </summary>
    public async ValueTask SetHtmlAsync(ElementReference element, string html)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setHtml", element, html);
    }

    /// <summary>
    /// Wstawia HTML w bieżącej pozycji kursora.
    /// </summary>
    public async ValueTask InsertHtmlAsync(string html)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertHtml", html);
    }

    /// <summary>
    /// Stosuje wyrównanie tekstu.
    /// </summary>
    public async ValueTask AlignTextAsync(string alignment)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("alignText", alignment);
    }

    /// <summary>
    /// Sprawdza czy polecenie jest aktywne.
    /// </summary>
    public async ValueTask<bool> QueryCommandStateAsync(string command)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("queryCommandState", command);
    }

    /// <summary>
    /// Tworzy link z bieżącego zaznaczenia.
    /// </summary>
    public async ValueTask CreateLinkAsync(string url)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("createLink", url);
    }

    /// <summary>
    /// Wstawia obraz.
    /// </summary>
    public async ValueTask InsertImageAsync(string src)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertImage", src);
    }

    /// <summary>
    /// Formatuje bieżący blok jako nagłówek.
    /// </summary>
    public async ValueTask FormatBlockAsync(string level)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("formatBlock", level);
    }

    public async ValueTask InsertHorizontalRuleAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertHorizontalRule");
    }

    public async ValueTask InsertOrderedListAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertOrderedList");
    }

    public async ValueTask InsertUnorderedListAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertUnorderedList");
    }

    public async ValueTask IndentAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("indent");
    }

    // ============================================================
    // Sekcja 3: Resize obrazków
    // ============================================================

    /// <summary>
    /// Inicjalizuje obsługę resize obrazków w kontenerze WYSIWYG.
    /// </summary>
    public async ValueTask InitImageResizeAsync<T>(ElementReference container, DotNetObjectReference<T> dotNetRef) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("initImageResize", container, dotNetRef);
    }

    /// <summary>
    /// Zwalnia zasoby modułu resize obrazków.
    /// </summary>
    public async ValueTask DisposeImageResizeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("disposeImageResize");
            }
            catch (JSDisconnectedException)
            {
                // Obwód już rozłączony
            }
        }
    }

    /// <summary>
    /// Ustawia wymiary zaznaczonego obrazka (wywoływane z popupu Blazor).
    /// </summary>
    public async ValueTask SetSelectedImageSizeAsync(int width, int height)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setSelectedImageSize", width, height);
    }

    /// <summary>
    /// Odznacza zaznaczony obrazek.
    /// </summary>
    public async ValueTask DeselectImageAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("deselectImage");
    }

    // ============================================================
    // Dispose
    // ============================================================

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await DisposeImageResizeAsync();

        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Obwód już rozłączony
            }
        }
    }
}
