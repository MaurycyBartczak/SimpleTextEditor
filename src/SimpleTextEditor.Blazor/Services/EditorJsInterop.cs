using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SimpleTextEditor.Blazor.Services;

/// <summary>
/// Wrapper C# dla funkcji JavaScript interop edytora.
/// </summary>
public class EditorJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private bool _isDisposed;
    
    public EditorJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SimpleTextEditor.Blazor/js/editor-interop.js");
        }
        return _module;
    }
    
    /// <summary>
    /// Pobiera bieżące zaznaczenie tekstu w edytorze.
    /// </summary>
    public async ValueTask<TextSelection> GetSelectionAsync(ElementReference textarea)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<TextSelection>("getSelection", textarea);
    }
    
    /// <summary>
    /// Ustawia pozycję kursora lub zaznaczenie w edytorze.
    /// </summary>
    public async ValueTask SetSelectionAsync(ElementReference textarea, int start, int? end = null)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setSelection", textarea, start, end ?? start);
    }
    
    /// <summary>
    /// Wstawia tekst w bieżącej pozycji kursora lub opakowuje zaznaczony tekst.
    /// </summary>
    public async ValueTask<string> InsertTextAsync(
        ElementReference textarea, 
        string? before, 
        string? after, 
        bool newLineBefore = false)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("insertText", 
            textarea, before ?? "", after ?? "", newLineBefore);
    }
    
    /// <summary>
    /// Opakowuje zaznaczony tekst ciągami przed/po.
    /// </summary>
    public async ValueTask<string> WrapSelectionAsync(
        ElementReference textarea, 
        string before, 
        string after)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("wrapSelection", textarea, before, after);
    }
    
    /// <summary>
    /// Pobiera bieżący numer linii (indeksowany od 1).
    /// </summary>
    public async ValueTask<int> GetCurrentLineAsync(ElementReference textarea)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<int>("getCurrentLine", textarea);
    }
    
    /// <summary>
    /// Synchronizuje pozycję przewijania między edytorem a podglądem.
    /// </summary>
    public async ValueTask SyncScrollAsync(ElementReference editor, ElementReference preview)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("syncScroll", editor, preview);
    }
    
    /// <summary>
    /// Przełącza tryb pełnoekranowy.
    /// </summary>
    public async ValueTask ToggleFullscreenAsync(ElementReference container, bool enable)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("toggleFullscreen", container, enable);
    }
    
    /// <summary>
    /// Ustawia fokus na polu tekstowym edytora.
    /// </summary>
    public async ValueTask FocusAsync(ElementReference textarea)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("focus", textarea);
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

/// <summary>
/// Reprezentuje zaznaczenie tekstu w edytorze.
/// </summary>
public record TextSelection(int Start, int End, string SelectedText);
