namespace SimpleTextEditor.Radzen.Components;

using Microsoft.AspNetCore.Components;
using Radzen;
using SimpleTextEditor.Core.Abstractions;
using SimpleTextEditor.Core.Models;
using SimpleTextEditor.Core.Providers;
using SimpleTextEditor.Core.Services;

/// <summary>
/// Komponent paska narzędzi w stylu Radzen wyświetlający przyciski formatowania z komponentami RadzenButton.
/// </summary>
public partial class RadzenToolbar : ComponentBase
{
    /// <summary>
    /// Elementy paska narzędzi do wyświetlenia.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ToolbarItem> Items { get; set; } = ToolbarItems.Default;
    
    /// <summary>
    /// Dostawca ikon do tłumaczenia nazw ikon na znaki wyświetlane.
    /// </summary>
    [Parameter]
    public IIconProvider IconProvider { get; set; } = new MaterialIconProvider();
    
    /// <summary>
    /// Dostawca lokalizacji dla podpowiedzi i etykiet.
    /// </summary>
    [Parameter]
    public ILocalizationProvider LocalizationProvider { get; set; } = new DefaultLocalizationProvider();
    
    /// <summary>
    /// Callback wywoływany po kliknięciu elementu paska narzędzi.
    /// </summary>
    [Parameter]
    public EventCallback<ToolbarItem> OnClick { get; set; }
    
    /// <summary>
    /// Funkcja sprawdzająca czy element jest aktywny/wciśnięty.
    /// </summary>
    [Parameter]
    public Func<string, bool>? ActiveCheck { get; set; }
    
    /// <summary>
    /// Funkcja sprawdzająca czy element jest wyłączony.
    /// </summary>
    [Parameter]
    public Func<string, bool>? DisabledCheck { get; set; }
    
    /// <summary>
    /// Dodatkowa klasa CSS dla kontenera paska narzędzi.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
    
    private string GetIcon(string iconName) => IconProvider.GetIcon(iconName);
    
    private string GetTooltip(ToolbarItem item)
    {
        var tooltip = LocalizationProvider.Get(item.TooltipKey);
        if (!string.IsNullOrEmpty(item.Shortcut))
        {
            tooltip += $" ({item.Shortcut})";
        }
        return tooltip;
    }
    
    private bool IsActive(string itemId) => ActiveCheck?.Invoke(itemId) ?? false;
    
    private bool IsDisabled(string itemId) => DisabledCheck?.Invoke(itemId) ?? false;
    
    private async Task OnItemClick(ToolbarItem item)
    {
        await OnClick.InvokeAsync(item);
    }
}
