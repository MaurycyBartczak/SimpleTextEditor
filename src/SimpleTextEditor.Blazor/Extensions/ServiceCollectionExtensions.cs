using Microsoft.Extensions.DependencyInjection;
using SimpleTextEditor.Core.Abstractions;
using SimpleTextEditor.Core.Models;
using SimpleTextEditor.Core.Providers;
using SimpleTextEditor.Core.Services;

namespace SimpleTextEditor.Blazor.Extensions;

/// <summary>
/// Metody rozszerzające do rejestracji usług SimpleTextEditor.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Dodaje bazowe usługi SimpleTextEditor do kolekcji serwisów.
    /// </summary>
    /// <param name="services">Kolekcja serwisów.</param>
    /// <param name="configure">Opcjonalna akcja konfiguracyjna.</param>
    /// <returns>Kolekcja serwisów do łańcuchowania wywołań.</returns>
    public static IServiceCollection AddSimpleTextEditor(
        this IServiceCollection services,
        Action<EditorOptions>? configure = null)
    {
        var options = new EditorOptions();
        configure?.Invoke(options);
        
        // Rejestracja opcji
        services.AddSingleton(options);
        
        // Rejestracja usług bazowych
        services.AddSingleton<IMarkdownParser, MarkdownService>();
        
        // Rejestracja dostawców
        if (options.IconProvider != null)
        {
            services.AddSingleton(options.IconProvider);
        }
        else
        {
            services.AddSingleton<IIconProvider, MaterialIconProvider>();
        }
        
        if (options.LocalizationProvider != null)
        {
            services.AddSingleton(options.LocalizationProvider);
        }
        else
        {
            var localization = new DefaultLocalizationProvider();
            localization.SetLanguage(options.Language);
            
            if (options.CustomTranslations != null)
            {
                localization.AddTranslations(options.CustomTranslations);
            }
            
            services.AddSingleton<ILocalizationProvider>(localization);
        }
        
        return services;
    }
}
