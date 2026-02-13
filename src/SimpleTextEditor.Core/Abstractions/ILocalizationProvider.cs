namespace SimpleTextEditor.Core.Abstractions;

/// <summary>
/// Interfejs dostarczający zlokalizowane teksty dla interfejsu edytora.
/// </summary>
public interface ILocalizationProvider
{
    /// <summary>
    /// Pobiera aktualny kod języka (np. "en", "pl").
    /// </summary>
    string CurrentLanguage { get; }
    
    /// <summary>
    /// Pobiera zlokalizowany tekst dla podanego klucza.
    /// </summary>
    /// <param name="key">Klucz lokalizacji (np. "bold", "italic", "insertImage").</param>
    /// <returns>Zlokalizowany tekst.</returns>
    string Get(string key);
    
    /// <summary>
    /// Pobiera wszystkie dostępne kody języków.
    /// </summary>
    /// <returns>Kolekcja kodów języków.</returns>
    IEnumerable<string> GetAvailableLanguages();
    
    /// <summary>
    /// Ustawia aktualny język.
    /// </summary>
    /// <param name="languageCode">Kod języka do ustawienia.</param>
    void SetLanguage(string languageCode);
    
    /// <summary>
    /// Dodaje lub nadpisuje tłumaczenia dla aktualnego języka.
    /// </summary>
    /// <param name="translations">Słownik par klucz-wartość tłumaczeń.</param>
    void AddTranslations(IDictionary<string, string> translations);
}
