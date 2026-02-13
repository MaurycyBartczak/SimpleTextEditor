using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SimpleTextEditor.Blazor.Services;

/// <summary>
/// Wrapper C# dla funkcji JavaScript interop trybu WYSIWYG.
/// </summary>
public class WysiwygJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private bool _isDisposed;
    
    public WysiwygJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SimpleTextEditor.Blazor/js/wysiwyg-interop.js");
        }
        return _module;
    }
    
    /// <summary>
    /// Wykonuje polecenie dokumentu (pogrubienie, kursywa itp.)
    /// </summary>
    public async ValueTask ExecCommandAsync(string command, string? value = null)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("execCommand", command, value);
    }
    
    /// <summary>
    /// Pobiera zawartość HTML z elementu contenteditable.
    /// </summary>
    public async ValueTask<string> GetHtmlAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("getHtml", element);
    }
    
    /// <summary>
    /// Ustawia zawartość HTML elementu contenteditable.
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
    /// Ustawia fokus na elemencie contenteditable.
    /// </summary>
    public async ValueTask FocusAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("focus", element);
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
    
    /// <summary>
    /// Wstawia linię poziomą.
    /// </summary>
    public async ValueTask InsertHorizontalRuleAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertHorizontalRule");
    }
    
    /// <summary>
    /// Przełącza listę numerowaną.
    /// </summary>
    public async ValueTask InsertOrderedListAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertOrderedList");
    }
    
    /// <summary>
    /// Przełącza listę punktowaną.
    /// </summary>
    public async ValueTask InsertUnorderedListAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("insertUnorderedList");
    }
    
    /// <summary>
    /// Dodaje wcięcie do bieżącego zaznaczenia (dla cytatu blokowego).
    /// </summary>
    public async ValueTask IndentAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("indent");
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Obwód już rozłączony, ignoruj
            }
        }
    }
}
