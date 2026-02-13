using Microsoft.Extensions.DependencyInjection;
using SimpleTextEditor.Blazor.Extensions;
using SimpleTextEditor.Core.Models;

namespace SimpleTextEditor.Radzen.Extensions;

/// <summary>
/// Metody rozszerzające do rejestracji usług edytora Markdown Radzen.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Dodaje usługi edytora Markdown Radzen do kolekcji serwisów.
    /// </summary>
    /// <param name="services">Kolekcja serwisów.</param>
    /// <param name="configure">Opcjonalna akcja konfiguracyjna.</param>
    /// <returns>Kolekcja serwisów do łańcuchowania wywołań.</returns>
    public static IServiceCollection AddRadzenMarkdownEditor(
        this IServiceCollection services,
        Action<EditorOptions>? configure = null)
    {
        // Rejestracja bazowych usług SimpleTextEditor
        services.AddSimpleTextEditor(configure);
        
        return services;
    }
}
