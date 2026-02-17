# SimpleTextEditor - Dokumentacja (PL)

Biblioteka komponentów Blazor do edycji tekstu w formacie Markdown z trybem WYSIWYG.

---

## Spis treści

1. [Instalacja](#instalacja)
2. [Szybki start](#szybki-start)
3. [Komponenty](#komponenty)
   - [RadzenMarkdownEditor](#radzenmarkdowneditor)
   - [EditorBase](#editorbase)
4. [Interfejsy](#interfejsy)
   - [IImageUploadHandler](#iimageuploadhandler)
   - [IMarkdownParser](#imarkdownparser)
   - [IIconProvider](#iiconprovider)
   - [ILocalizationProvider](#ilocalizationprovider)
   - [IEditorTheme](#ieditortheme)
5. [Modele](#modele)
   - [EditorMode](#editormode)
   - [PreviewMode](#previewmode)
   - [ToolbarItem](#toolbaritem)
   - [ToolbarItems (predefiniowane)](#toolbaritems-predefiniowane)
6. [Architektura JavaScript](#architektura-javascript)
7. [Zmiana rozmiaru obrazów](#zmiana-rozmiaru-obrazów)
8. [Pasek statusu](#pasek-statusu)
9. [Skróty klawiaturowe WYSIWYG](#skróty-klawiaturowe-wysiwyg)
10. [Drag & drop i wklejanie obrazków (WYSIWYG)](#drag--drop-i-wklejanie-obrazków-wysiwyg)
11. [Bezpieczeństwo (Security hardening)](#bezpieczeństwo-security-hardening)
12. [Przykłady użycia](#przykłady-użycia)

---

## Instalacja

```bash
dotnet add package SimpleTextEditor.Radzen
```

Dodaj do `_Imports.razor`:
```razor
@using SimpleTextEditor.Radzen.Components
@using SimpleTextEditor.Core.Models
@using SimpleTextEditor.Core.Abstractions
```

---

## Szybki start

```razor
@page "/edytor"

<RadzenMarkdownEditor @bind-Value="content" />

@code {
    private string content = "# Witaj świecie!";
}
```

---

## Komponenty

### RadzenMarkdownEditor

Główny komponent edytora z obsługą trybu WYSIWYG i Markdown.

#### Szczegółowy opis parametrów

| Parametr | Typ | Domyślna wartość | Opis szczegółowy |
|----------|-----|------------------|------------------|
| `Value` | `string` | `""` | **Zawartość edytora w formacie Markdown.** Jest to główna właściwość przechowująca tekst. Używaj z `@bind-Value` dla two-way binding lub przekaż wartość i `ValueChanged` osobno. |
| `ValueChanged` | `EventCallback<string>` | - | **Callback wywoływany przy każdej zmianie wartości.** Automatycznie używany przy `@bind-Value`. Pozwala reagować na zmiany w czasie rzeczywistym. |
| `Mode` | `EditorMode` | `Wysiwyg` | **Początkowy tryb edycji.** `Wysiwyg` - edytor wizualny (WYSIWYG), `Markdown` - edycja surowego kodu Markdown. Użytkownik może przełączać tryby przyciskiem w pasku narzędzi. |
| `PreviewMode` | `PreviewMode` | `SideBySide` | **Tryb podglądu (tylko w trybie Markdown).** `None` - bez podglądu, `SideBySide` - edytor i podgląd obok siebie (50/50), `Toggle` - przełączanie między edytorem a podglądem. |
| `Theme` | `string` | `"light"` | **Nazwa motywu kolorystycznego.** Dostępne wartości: `"light"` (jasny), `"dark"` (ciemny). Można też użyć parametru `EditorTheme` dla własnego motywu. |
| `Placeholder` | `string?` | `null` | **Tekst zastępczy wyświetlany gdy edytor jest pusty.** Jeśli `null`, używa domyślnego z `LocalizationProvider` (klucz `"placeholder"`). |
| `MinHeight` | `int` | `300` | **Minimalna wysokość edytora w pikselach.** Edytor nie zmniejszy się poniżej tej wartości nawet przy braku treści. |
| `MaxHeight` | `int` | `0` | **Maksymalna wysokość edytora w pikselach.** `0` oznacza brak limitu - edytor rośnie wraz z treścią. Przy wartości > 0 pojawia się pasek przewijania. |
| `ReadOnly` | `bool` | `false` | **Tryb tylko do odczytu.** Gdy `true`, użytkownik nie może edytować treści. Pasek narzędzi jest ukryty. Przydatne do wyświetlania artykułów. |
| `CssClass` | `string?` | `null` | **Dodatkowa klasa CSS dla kontenera edytora.** Pozwala na własne stylowanie, np. `"my-custom-editor border-primary"`. |
| `ToolbarItems` | `IReadOnlyList<ToolbarItem>?` | `null` | **Własna konfiguracja paska narzędzi.** Gdy `null`, używa domyślnego zestawu (`ToolbarItems.Default`). Pozwala usunąć niechciane przyciski lub dodać własne. |
| `ImageUploadHandler` | `IImageUploadHandler?` | `null` | **Handler uploadu obrazów.** Gdy `null`, używa `Base64ImageUploadHandler` - obrazy są konwertowane na Base64 i osadzane w treści. Zaimplementuj własny handler dla Azure Blob, S3, itp. |
| `HtmlToMarkdownConverter` | `IHtmlToMarkdownConverter?` | `null` | **Konwerter HTML → Markdown (WYSIWYG).** Gdy `null`, używa domyślnego `HtmlToMarkdownConverter`. Zaimplementuj własny jeśli potrzebujesz niestandardowej konwersji z trybu WYSIWYG na Markdown. |
| `IconProvider` | `IIconProvider?` | `null` | **Provider ikon dla paska narzędzi.** Gdy `null`, używa `MaterialIconProvider` (Material Icons). Zaimplementuj własny dla FontAwesome, Bootstrap Icons, itp. |
| `LocalizationProvider` | `ILocalizationProvider?` | `null` | **Provider tłumaczeń interfejsu.** Gdy `null`, używa `DefaultLocalizationProvider` (angielski). Zaimplementuj własny dla polskiego lub innych języków. |
| `MarkdownParser` | `IMarkdownParser?` | `null` | **Parser Markdown do HTML.** Gdy `null`, używa `MarkdownService` opartego na bibliotece Markdig. Zaimplementuj własny jeśli potrzebujesz innej składni. |
| `EditorTheme` | `IEditorTheme?` | `null` | **Własna instancja motywu.** Ma priorytet nad parametrem `Theme`. Pozwala na pełną kontrolę nad stylami CSS edytora. |
| `OnChange` | `EventCallback<string>` | - | **Callback przy każdej zmianie zawartości.** Podobny do `ValueChanged`, ale wywoływany niezależnie. Można użyć obu jednocześnie. |

#### Przykład z wszystkimi opcjami

```razor
@page "/pelny-edytor"
@using SimpleTextEditor.Radzen.Components
@using SimpleTextEditor.Core.Models
@using SimpleTextEditor.Core.Abstractions

<h3>Pełny przykład edytora</h3>

<RadzenMarkdownEditor 
    @bind-Value="content"
    Mode="EditorMode.Wysiwyg"
    PreviewMode="PreviewMode.SideBySide"
    Theme="dark"
    Placeholder="Zacznij pisać artykuł..."
    MinHeight="400"
    MaxHeight="800"
    ReadOnly="false"
    CssClass="my-custom-editor shadow-lg"
    ToolbarItems="customToolbar"
    ImageUploadHandler="imageHandler"
    IconProvider="iconProvider"
    LocalizationProvider="localizationProvider"
    EditorTheme="customTheme"
    OnChange="HandleContentChange" />

<div class="mt-3">
    <strong>Liczba znaków:</strong> @content.Length
</div>

@code {
    private string content = "# Mój artykuł\n\nTreść artykułu...";
    
    // Własny handler obrazów - zapisuje do Azure Blob Storage
    private IImageUploadHandler imageHandler = new AzureBlobImageHandler(
        connectionString: "DefaultEndpointsProtocol=https;AccountName=...",
        containerName: "images"
    );
    
    // Własny provider ikon - FontAwesome zamiast Material Icons
    private IIconProvider iconProvider = new FontAwesomeIconProvider();
    
    // Polski interfejs
    private ILocalizationProvider localizationProvider = new PolishLocalizationProvider();
    
    // Własny motyw z niestandardowymi stylami
    private IEditorTheme customTheme = new CustomDarkTheme();
    
    // Własny pasek narzędzi - tylko podstawowe formatowanie
    private IReadOnlyList<ToolbarItem> customToolbar = new[]
    {
        ToolbarItems.Bold,
        ToolbarItems.Italic,
        ToolbarItems.Strikethrough,
        ToolbarItem.Separator,
        ToolbarItems.Heading1,
        ToolbarItems.Heading2,
        ToolbarItem.Separator,
        ToolbarItems.BulletList,
        ToolbarItems.NumberedList,
        ToolbarItem.Separator,
        ToolbarItems.Link,
        ToolbarItems.Image,
        ToolbarItem.Separator,
        ToolbarItems.SwitchMode
    };
    
    private void HandleContentChange(string newContent)
    {
        Console.WriteLine($"Treść zmieniona: {newContent.Length} znaków");
        // Możesz tu dodać auto-save, walidację, itp.
    }
}
```

---

### EditorBase

Podstawowy edytor Markdown bez stylów Radzen. Używaj gdy:
- Chcesz własne stylowanie CSS
- Nie potrzebujesz komponentów Radzen
- Budujesz edytor jako bazę dla innego frameworka UI

#### Parametry EditorBase

Takie same jak `RadzenMarkdownEditor`, z następującymi różnicami:

| Parametr | Różnica vs RadzenMarkdownEditor |
|----------|----------------------------------|
| `Mode` | **Brak** - EditorBase obsługuje tylko tryb Markdown |

---

## Interfejsy

### IImageUploadHandler

Interfejs do obsługi uploadu obrazów. **Domyślnie obrazy są konwertowane na Base64** i osadzane bezpośrednio w treści Markdown, co ma wady:
- Duży rozmiar dokumentu
- Wolne ładowanie
- Duplikacja przy kopiowaniu

Zaimplementuj własny handler aby przechowywać obrazy w:
- **Baza danych** - najprostsze podejście, wszystko w jednym miejscu
- **System plików** - dla prostych aplikacji i lokalnych deployów
- **AWS S3 / CDN** - dla skalowalnych aplikacji w chmurze
- **Azure Blob Storage** - dla aplikacji w ekosystemie Azure

#### Sygnatura interfejsu

```csharp
public interface IImageUploadHandler
{
    /// <summary>
    /// Zapisuje obraz i zwraca URL do użycia w edytorze.
    /// </summary>
    /// <param name="fileName">Oryginalna nazwa pliku, np. "zdjecie.png"</param>
    /// <param name="content">Zawartość pliku jako tablica bajtów</param>
    /// <param name="contentType">Typ MIME, np. "image/png", "image/jpeg"</param>
    /// <returns>URL obrazu do wstawienia w Markdown, np. "https://cdn.example.com/img/abc123.png"</returns>
    Task<string> UploadAsync(string fileName, byte[] content, string contentType);
    
    /// <summary>
    /// Maksymalny dozwolony rozmiar pliku w bajtach.
    /// Domyślnie: 10 MB (10 * 1024 * 1024)
    /// Zwróć 0 dla braku limitu (niezalecane).
    /// </summary>
    long MaxFileSizeBytes => 10 * 1024 * 1024;
    
    /// <summary>
    /// Lista dozwolonych typów MIME.
    /// Domyślnie: JPEG, PNG, GIF, WebP (SVG wyłączone ze względów bezpieczeństwa).
    /// </summary>
    IReadOnlyList<string> AllowedContentTypes => new[]
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };
}
```

#### Klasa bazowa `ImageUploadHandlerBase` (zalecana)

Zamiast implementować `IImageUploadHandler` bezpośrednio, **zalecam dziedziczenie z `ImageUploadHandlerBase`**. Klasa bazowa zapewnia:
- ✅ Walidację rozmiaru pliku
- ✅ Walidację typu MIME
- ✅ Walidację magic bytes (sygnatura pliku musi zgadzać się z deklarowanym typem MIME)
- ✅ Generowanie unikalnych nazw plików (Guid) z rozszerzeniem na podstawie MIME (nie nazwy od klienta)
- ✅ Mapowanie MIME → rozszerzenie
- ✅ Domyślne blokowanie SVG

Wystarczy zaimplementować jedną metodę `SaveAsync()`:

```csharp
using SimpleTextEditor.Core.Services;

public class MyImageHandler : ImageUploadHandlerBase
{
    protected override Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
    {
        // Tu tylko logika zapisu – walidacja jest już obsłużona!
        // uniqueFileName to np. "a1b2c3d4-e5f6-7890-abcd-ef1234567890.png"
        throw new NotImplementedException();
    }
}
```

#### Przykład 1: Baza danych (Entity Framework)

Najprostsze podejście – obraz jako `byte[]` w tabeli. Idealne gdy nie chcesz konfigurować zewnętrznego storage.

```csharp
using SimpleTextEditor.Core.Services;

public class DatabaseImageHandler : ImageUploadHandlerBase
{
    private readonly AppDbContext _dbContext;
    private readonly string _baseUrl;
    
    public DatabaseImageHandler(AppDbContext dbContext, string baseUrl)
    {
        _dbContext = dbContext;
        _baseUrl = baseUrl;
    }
    
    protected override async Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
    {
        var image = new ImageEntity
        {
            Id = Guid.NewGuid(),
            FileName = uniqueFileName,
            ContentType = contentType,
            Data = content,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();
        
        return $"{_baseUrl}/api/images/{image.Id}";
    }
}

// Kontroler do serwowania obrazów
[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var image = await _dbContext.Images.FindAsync(id);
        if (image == null) return NotFound();
        
        return File(image.Data, image.ContentType);
    }
}
```

#### Przykład 2: System plików

Zapisuje pliki na dysku, organizując je w foldery wg daty. Dobre dla prostych deployów.

```csharp
using SimpleTextEditor.Core.Services;

public class FileSystemImageHandler : ImageUploadHandlerBase
{
    private readonly string _uploadPath;
    private readonly string _urlPrefix;
    
    public FileSystemImageHandler(string uploadPath, string urlPrefix)
    {
        _uploadPath = uploadPath;
        _urlPrefix = urlPrefix.TrimEnd('/');
        Directory.CreateDirectory(uploadPath);
    }
    
    protected override async Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
    {
        // Organizuj w foldery według daty
        var dateFolder = DateTime.UtcNow.ToString("yyyy-MM");
        var targetDir = Path.Combine(_uploadPath, dateFolder);
        Directory.CreateDirectory(targetDir);
        
        var filePath = Path.Combine(targetDir, uniqueFileName);
        await File.WriteAllBytesAsync(filePath, content);
        
        return $"{_urlPrefix}/{dateFolder}/{uniqueFileName}";
    }
    
    // Ogranicz do 2 MB
    public override long MaxFileSizeBytes => 2 * 1024 * 1024;
}
```

#### Przykład 3: AWS S3 / CDN

Upload do Amazon S3 z opcjonalnym CDN (CloudFront). Wymaga pakietu `AWSSDK.S3`.

```csharp
using Amazon.S3;
using Amazon.S3.Model;
using SimpleTextEditor.Core.Services;

public class S3ImageHandler : ImageUploadHandlerBase
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _cdnBaseUrl;
    
    public S3ImageHandler(IAmazonS3 s3Client, string bucketName, string cdnBaseUrl)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
        _cdnBaseUrl = cdnBaseUrl.TrimEnd('/');
    }
    
    protected override async Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
    {
        var key = $"images/{DateTime.UtcNow:yyyy/MM/dd}/{uniqueFileName}";
        
        using var stream = new MemoryStream(content);
        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            Headers = { CacheControl = "public, max-age=31536000" }
        });
        
        return $"{_cdnBaseUrl}/{key}";
    }
    
    // Ogranicz do 5 MB
    public override long MaxFileSizeBytes => 5 * 1024 * 1024;
}
```

#### Przykład 4: Azure Blob Storage

Upload do Azure Blob z opcjonalnym CDN. Wymaga pakietu `Azure.Storage.Blobs`.

```csharp
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SimpleTextEditor.Core.Services;

public class AzureBlobImageHandler : ImageUploadHandlerBase
{
    private readonly BlobContainerClient _container;
    private readonly string _cdnBaseUrl;
    
    public AzureBlobImageHandler(string connectionString, string containerName, string? cdnBaseUrl = null)
    {
        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
        _cdnBaseUrl = cdnBaseUrl ?? _container.Uri.ToString();
    }
    
    protected override async Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
    {
        var blobName = $"{DateTime.UtcNow:yyyy/MM/dd}/{uniqueFileName}";
        var blob = _container.GetBlobClient(blobName);
        
        await blob.UploadAsync(
            new BinaryData(content), 
            new BlobUploadOptions 
            { 
                HttpHeaders = new BlobHttpHeaders 
                { 
                    ContentType = contentType,
                    CacheControl = "public, max-age=31536000"
                } 
            });
        
        return $"{_cdnBaseUrl}/{blobName}";
    }
    
    // Ogranicz do 5 MB
    public override long MaxFileSizeBytes => 5 * 1024 * 1024;
}
```

#### Rejestracja handlera w DI

```csharp
// Program.cs

// Opcja 1: Baza danych (najczęstszy scenariusz)
builder.Services.AddScoped<IImageUploadHandler, DatabaseImageHandler>();

// Opcja 2: System plików
builder.Services.AddSingleton<IImageUploadHandler>(
    new FileSystemImageHandler(
        Path.Combine(builder.Environment.WebRootPath, "uploads"),
        "/uploads"
    ));

// Opcja 3: AWS S3
builder.Services.AddSingleton<IImageUploadHandler>(sp =>
    new S3ImageHandler(
        sp.GetRequiredService<IAmazonS3>(),
        builder.Configuration["AWS:BucketName"]!,
        builder.Configuration["AWS:CdnUrl"]!
    ));

// Opcja 4: Azure Blob
builder.Services.AddSingleton<IImageUploadHandler>(sp =>
    new AzureBlobImageHandler(
        builder.Configuration["Azure:StorageConnectionString"]!,
        "editor-images",
        builder.Configuration["Azure:CdnUrl"]
    ));
```

---

### IMarkdownParser

Interfejs do konwersji Markdown ↔ HTML.

#### Sygnatura

```csharp
public interface IMarkdownParser
{
    /// <summary>
    /// Konwertuje Markdown do HTML.
    /// </summary>
    /// <param name="markdown">Tekst w formacie Markdown</param>
    /// <returns>HTML gotowy do wyświetlenia</returns>
    string ToHtml(string markdown);
    
    /// <summary>
    /// Konwertuje Markdown do czystego tekstu (usuwa formatowanie).
    /// Przydatne do wyszukiwania, podglądów, SEO.
    /// </summary>
    string ToPlainText(string markdown);
}
```

#### Domyślna implementacja

Domyślnie używany jest `MarkdownService` z biblioteką **Markdig** obsługującą:
- Tabele
- Listy zadań (checkboxy)
- Automatyczne linki
- Kod z podświetlaniem składni
- Emoji
- Footnotes
- **Sanitizację HTML** — wbudowana warstwa bezpieczeństwa (HtmlSanitizer) neutralizuje XSS payloady (`<script>`, `onerror`, `javascript:` itp.) zachowując bezpieczne formatowanie

---

### IIconProvider

Interfejs do dostarczania ikon dla paska narzędzi.

#### Sygnatura

```csharp
public interface IIconProvider
{
    /// <summary>
    /// Zwraca identyfikator ikony dla danej akcji.
    /// </summary>
    /// <param name="actionName">Nazwa akcji, np. "bold", "italic", "heading1"</param>
    /// <returns>
    /// Zależnie od implementacji:
    /// - Material Icons: "format_bold"
    /// - FontAwesome: "fa-bold"
    /// - Bootstrap Icons: "bi-type-bold"
    /// - SVG: "<svg>...</svg>"
    /// </returns>
    string GetIcon(string actionName);
    
    /// <summary>
    /// Zwraca link do czcionki ikon do dodania w <head>.
    /// Zwróć pusty string jeśli ikony są już załadowane przez aplikację.
    /// </summary>
    string GetIconFontLink();
}
```

#### Lista obsługiwanych nazw akcji

| actionName | Opis |
|------------|------|
| `bold` | Pogrubienie |
| `italic` | Kursywa |
| `strikethrough` | Przekreślenie |
| `heading1` | Nagłówek H1 |
| `heading2` | Nagłówek H2 |
| `heading3` | Nagłówek H3 |
| `bulletList` | Lista punktowana |
| `numberedList` | Lista numerowana |
| `quote` | Cytat |
| `code` | Kod inline |
| `codeBlock` | Blok kodu |
| `link` | Link |
| `image` | Obraz |
| `table` | Tabela |
| `horizontalRule` | Linia pozioma |
| `undo` | Cofnij |
| `redo` | Ponów |
| `preview` | Podgląd |
| `fullscreen` | Pełny ekran |
| `switchMode` | Przełącz tryb |
| `alignLeft` | Wyrównaj do lewej |
| `alignCenter` | Wyrównaj do środka |
| `alignRight` | Wyrównaj do prawej |

#### Przykład: FontAwesome 6

```csharp
public class FontAwesomeIconProvider : IIconProvider
{
    private readonly Dictionary<string, string> _icons = new()
    {
        // Formatowanie tekstu
        ["bold"] = "fa-solid fa-bold",
        ["italic"] = "fa-solid fa-italic",
        ["strikethrough"] = "fa-solid fa-strikethrough",
        
        // Nagłówki
        ["heading1"] = "fa-solid fa-heading",
        ["heading2"] = "fa-solid fa-h",
        ["heading3"] = "fa-solid fa-h",
        
        // Listy
        ["bulletList"] = "fa-solid fa-list-ul",
        ["numberedList"] = "fa-solid fa-list-ol",
        
        // Bloki
        ["quote"] = "fa-solid fa-quote-left",
        ["code"] = "fa-solid fa-code",
        ["codeBlock"] = "fa-solid fa-file-code",
        
        // Wstawianie
        ["link"] = "fa-solid fa-link",
        ["image"] = "fa-solid fa-image",
        ["table"] = "fa-solid fa-table",
        ["horizontalRule"] = "fa-solid fa-minus",
        
        // Akcje
        ["undo"] = "fa-solid fa-rotate-left",
        ["redo"] = "fa-solid fa-rotate-right",
        ["preview"] = "fa-solid fa-eye",
        ["fullscreen"] = "fa-solid fa-expand",
        ["switchMode"] = "fa-solid fa-repeat",
        
        // Wyrównanie
        ["alignLeft"] = "fa-solid fa-align-left",
        ["alignCenter"] = "fa-solid fa-align-center",
        ["alignRight"] = "fa-solid fa-align-right"
    };
    
    public string GetIcon(string actionName) => 
        _icons.TryGetValue(actionName, out var icon) ? icon : "fa-solid fa-question";
    
    public string GetIconFontLink() => 
        "<link href=\"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css\" rel=\"stylesheet\" />";
}
```

#### Przykład: Bootstrap Icons

```csharp
public class BootstrapIconProvider : IIconProvider
{
    private readonly Dictionary<string, string> _icons = new()
    {
        ["bold"] = "bi-type-bold",
        ["italic"] = "bi-type-italic",
        ["strikethrough"] = "bi-type-strikethrough",
        ["heading1"] = "bi-type-h1",
        ["heading2"] = "bi-type-h2",
        ["heading3"] = "bi-type-h3",
        ["bulletList"] = "bi-list-ul",
        ["numberedList"] = "bi-list-ol",
        ["quote"] = "bi-quote",
        ["code"] = "bi-code",
        ["codeBlock"] = "bi-code-square",
        ["link"] = "bi-link-45deg",
        ["image"] = "bi-image",
        ["table"] = "bi-table",
        ["horizontalRule"] = "bi-dash-lg",
        ["undo"] = "bi-arrow-counterclockwise",
        ["redo"] = "bi-arrow-clockwise",
        ["preview"] = "bi-eye",
        ["fullscreen"] = "bi-fullscreen",
        ["switchMode"] = "bi-arrow-repeat"
    };
    
    public string GetIcon(string actionName) => 
        _icons.TryGetValue(actionName, out var icon) ? icon : "bi-question-circle";
    
    public string GetIconFontLink() => 
        "<link href=\"https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css\" rel=\"stylesheet\" />";
}
```

---

### ILocalizationProvider

Interfejs do tłumaczeń interfejsu użytkownika edytora.

#### Sygnatura

```csharp
public interface ILocalizationProvider
{
    /// <summary>
    /// Aktualny kod języka (np. "pl", "en", "de").
    /// </summary>
    string CurrentLanguage { get; }
    
    /// <summary>
    /// Pobiera przetłumaczony tekst dla klucza.
    /// </summary>
    /// <param name="key">Klucz tłumaczenia</param>
    /// <returns>Przetłumaczony tekst lub klucz jeśli brak tłumaczenia</returns>
    string Get(string key);
    
    /// <summary>
    /// Lista dostępnych języków.
    /// </summary>
    IEnumerable<string> GetAvailableLanguages();
    
    /// <summary>
    /// Zmienia aktualny język.
    /// </summary>
    void SetLanguage(string languageCode);
    
    /// <summary>
    /// Dodaje lub nadpisuje tłumaczenia.
    /// </summary>
    void AddTranslations(IDictionary<string, string> translations);
}
```

#### Lista kluczy tłumaczeń

| Klucz | Opis | Przykład PL |
|-------|------|-------------|
| `bold` | Tooltip: Pogrubienie | "Pogrubienie" |
| `italic` | Tooltip: Kursywa | "Kursywa" |
| `strikethrough` | Tooltip: Przekreślenie | "Przekreślenie" |
| `heading1` | Tooltip: Nagłówek 1 | "Nagłówek 1" |
| `heading2` | Tooltip: Nagłówek 2 | "Nagłówek 2" |
| `heading3` | Tooltip: Nagłówek 3 | "Nagłówek 3" |
| `bulletList` | Tooltip: Lista punktowana | "Lista punktowana" |
| `numberedList` | Tooltip: Lista numerowana | "Lista numerowana" |
| `quote` | Tooltip: Cytat | "Cytat" |
| `code` | Tooltip: Kod | "Kod" |
| `codeBlock` | Tooltip: Blok kodu | "Blok kodu" |
| `link` | Tooltip: Wstaw link | "Wstaw link" |
| `image` | Tooltip: Wstaw obraz | "Wstaw obraz" |
| `table` | Tooltip: Wstaw tabelę | "Wstaw tabelę" |
| `horizontalRule` | Tooltip: Linia pozioma | "Linia pozioma" |
| `undo` | Tooltip: Cofnij | "Cofnij" |
| `redo` | Tooltip: Ponów | "Ponów" |
| `preview` | Tooltip: Podgląd | "Podgląd" |
| `fullscreen` | Tooltip: Pełny ekran | "Pełny ekran" |
| `switchMode` | Tooltip: Przełącz tryb | "Przełącz tryb" |
| `placeholder` | Tekst zastępczy edytora | "Zacznij pisać..." |
| `noPreview` | Tekst gdy brak podglądu | "Brak treści do podglądu" |
| `words` | Etykieta paska statusu: słowa | "Słowa" |
| `characters` | Etykieta paska statusu: znaki | "Znaki" |
| `lines` | Etykieta paska statusu: linie | "Linie" |

#### Przykład: Pełna implementacja polska

```csharp
public class PolishLocalizationProvider : ILocalizationProvider
{
    private string _currentLanguage = "pl";
    
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["pl"] = new()
        {
            // Formatowanie
            ["bold"] = "Pogrubienie",
            ["italic"] = "Kursywa",
            ["strikethrough"] = "Przekreślenie",
            ["underline"] = "Podkreślenie",
            
            // Nagłówki
            ["heading1"] = "Nagłówek 1",
            ["heading2"] = "Nagłówek 2",
            ["heading3"] = "Nagłówek 3",
            
            // Listy
            ["bulletList"] = "Lista punktowana",
            ["numberedList"] = "Lista numerowana",
            ["taskList"] = "Lista zadań",
            
            // Bloki
            ["quote"] = "Cytat",
            ["code"] = "Kod",
            ["codeBlock"] = "Blok kodu",
            
            // Wstawianie
            ["link"] = "Wstaw link",
            ["image"] = "Wstaw obraz",
            ["table"] = "Wstaw tabelę",
            ["horizontalRule"] = "Linia pozioma",
            
            // Wyrównanie
            ["alignLeft"] = "Wyrównaj do lewej",
            ["alignCenter"] = "Wyśrodkuj",
            ["alignRight"] = "Wyrównaj do prawej",
            
            // Akcje
            ["undo"] = "Cofnij",
            ["redo"] = "Ponów",
            ["preview"] = "Podgląd",
            ["fullscreen"] = "Pełny ekran",
            ["switchMode"] = "Przełącz tryb",
            
            // Inne
            ["placeholder"] = "Zacznij pisać...",
            ["noPreview"] = "Brak treści do podglądu",
            ["words"] = "Słowa",
            ["characters"] = "Znaki",
            ["lines"] = "Linie",
            ["uploadImage"] = "Prześlij obraz",
            ["insertLink"] = "Wstaw link",
            ["linkUrl"] = "Adres URL",
            ["linkText"] = "Tekst linku"
        },
        ["en"] = new()
        {
            ["bold"] = "Bold",
            ["italic"] = "Italic",
            // ... angielskie tłumaczenia
        }
    };
    
    public string CurrentLanguage => _currentLanguage;
    
    public string Get(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var langDict) &&
            langDict.TryGetValue(key, out var value))
        {
            return value;
        }
        return key; // Fallback do klucza
    }
    
    public IEnumerable<string> GetAvailableLanguages() => _translations.Keys;
    
    public void SetLanguage(string languageCode)
    {
        if (_translations.ContainsKey(languageCode))
            _currentLanguage = languageCode;
    }
    
    public void AddTranslations(IDictionary<string, string> translations)
    {
        if (!_translations.ContainsKey(_currentLanguage))
            _translations[_currentLanguage] = new Dictionary<string, string>();
        
        foreach (var (key, value) in translations)
            _translations[_currentLanguage][key] = value;
    }
}
```

---

### IEditorTheme

Interfejs do definiowania własnych motywów edytora.

#### Sygnatura

```csharp
public interface IEditorTheme
{
    /// <summary>
    /// Nazwa motywu (identyfikator).
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Klasa CSS dla głównego kontenera edytora.
    /// </summary>
    string ContainerClass { get; }
    
    /// <summary>
    /// Klasa CSS dla paska narzędzi.
    /// </summary>
    string ToolbarClass { get; }
    
    /// <summary>
    /// Klasa CSS dla obszaru edycji (textarea/WYSIWYG).
    /// </summary>
    string EditorClass { get; }
    
    /// <summary>
    /// Klasa CSS dla panelu podglądu.
    /// </summary>
    string PreviewClass { get; }
    
    /// <summary>
    /// Dodatkowe style CSS inline (wstrzykiwane do strony).
    /// </summary>
    string AdditionalStyles { get; }
}
```

#### Przykład: Własny ciemny motyw z gradientem

```csharp
public class GradientDarkTheme : IEditorTheme
{
    public string Name => "gradient-dark";
    
    public string ContainerClass => "ste-container ste-theme-gradient-dark";
    
    public string ToolbarClass => "ste-toolbar gradient-toolbar";
    
    public string EditorClass => "ste-editor dark-editor";
    
    public string PreviewClass => "ste-preview dark-preview";
    
    public string AdditionalStyles => @"
        .ste-theme-gradient-dark {
            --ste-bg: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
            --ste-text: #e0e0e0;
            --ste-border: #3d5a80;
            --ste-accent: #00d4ff;
        }
        
        .ste-theme-gradient-dark .ste-container {
            background: var(--ste-bg);
            color: var(--ste-text);
            border: 1px solid var(--ste-border);
            border-radius: 12px;
            overflow: hidden;
        }
        
        .gradient-toolbar {
            background: rgba(0, 0, 0, 0.3);
            backdrop-filter: blur(10px);
            border-bottom: 1px solid var(--ste-border);
            padding: 8px;
        }
        
        .gradient-toolbar button {
            background: transparent;
            color: var(--ste-text);
            border: none;
            border-radius: 6px;
            padding: 8px;
            transition: all 0.2s;
        }
        
        .gradient-toolbar button:hover {
            background: var(--ste-accent);
            color: #000;
        }
        
        .dark-editor {
            background: transparent;
            color: var(--ste-text);
            font-family: 'JetBrains Mono', monospace;
        }
        
        .dark-preview {
            background: rgba(255, 255, 255, 0.05);
            padding: 20px;
        }
        
        .dark-preview h1, .dark-preview h2, .dark-preview h3 {
            color: var(--ste-accent);
        }
    ";
}
```

---

## Modele

### EditorMode

Enum określający tryb edytora.

```csharp
public enum EditorMode
{
    /// <summary>
    /// Tryb Markdown - użytkownik widzi i edytuje surowy kod Markdown.
    /// Przykład widoku: "# Nagłówek\n**pogrubiony tekst**"
    /// </summary>
    Markdown,
    
    /// <summary>
    /// Tryb WYSIWYG - użytkownik widzi sformatowany tekst jak w Word.
    /// Przykład widoku: nagłówek i pogrubiony tekst wyświetlone wizualnie.
    /// </summary>
    Wysiwyg
}
```

#### Kiedy używać którego trybu?

| Tryb | Zalety | Wady | Dla kogo? |
|------|--------|------|-----------|
| **Wysiwyg** | Intuicyjny, "widzisz co dostajesz" | Mniejsza kontrola nad formatowaniem | Użytkownicy nietechniczni |
| **Markdown** | Pełna kontrola, przenośność | Wymaga znajomości składni | Programiści, pisarze techniczni |

```razor
<!-- Dla użytkowników biznesowych -->
<RadzenMarkdownEditor Mode="EditorMode.Wysiwyg" />

<!-- Dla programistów/dokumentacji -->
<RadzenMarkdownEditor Mode="EditorMode.Markdown" PreviewMode="PreviewMode.SideBySide" />
```

---

### PreviewMode

Enum określający tryb podglądu (tylko w trybie Markdown).

```csharp
public enum PreviewMode
{
    /// <summary>
    /// Bez podglądu - tylko edytor Markdown.
    /// Zajmuje 100% szerokości.
    /// </summary>
    None,
    
    /// <summary>
    /// Edytor i podgląd obok siebie (50/50).
    /// Idealne dla szerokich ekranów.
    /// </summary>
    SideBySide,
    
    /// <summary>
    /// Przełączanie między edytorem a podglądem.
    /// Idealne dla wąskich ekranów/mobile.
    /// </summary>
    Toggle
}
```

#### Wizualne porównanie

```
┌─────────────────────────────────────────┐
│ PreviewMode.None                        │
├─────────────────────────────────────────┤
│                                         │
│         [Edytor Markdown 100%]          │
│                                         │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ PreviewMode.SideBySide                  │
├──────────────────┬──────────────────────┤
│                  │                      │
│  [Edytor 50%]    │   [Podgląd 50%]      │
│                  │                      │
└──────────────────┴──────────────────────┘

┌─────────────────────────────────────────┐
│ PreviewMode.Toggle    [📝] [👁]         │
├─────────────────────────────────────────┤
│                                         │
│   [Albo edytor ALBO podgląd 100%]       │
│                                         │
└─────────────────────────────────────────┘
```

---

### ToolbarItem

Klasa reprezentująca pojedynczy przycisk lub separator w pasku narzędzi.

#### Pełna definicja

```csharp
public class ToolbarItem
{
    /// <summary>
    /// Unikalny identyfikator akcji.
    /// Używany do rozpoznania klikniętego przycisku.
    /// Przykłady: "bold", "italic", "heading1", "myCustomAction"
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Nazwa/klasa ikony przekazywana do IIconProvider.GetIcon().
    /// Przykłady: "format_bold", "fa-bold", "bi-type-bold"
    /// </summary>
    public required string Icon { get; init; }
    
    /// <summary>
    /// Klucz używany do pobrania tłumaczenia z ILocalizationProvider.
    /// Wyświetlany jako tooltip przy najechaniu myszką.
    /// </summary>
    public required string TooltipKey { get; init; }
    
    /// <summary>
    /// Składnia Markdown wstawiana PRZED zaznaczonym tekstem.
    /// Przykłady: "**" (bold), "*" (italic), "# " (heading)
    /// </summary>
    public string? MarkdownBefore { get; init; }
    
    /// <summary>
    /// Składnia Markdown wstawiana PO zaznaczonym tekście.
    /// Przykłady: "**" (bold), "*" (italic), null (heading)
    /// </summary>
    public string? MarkdownAfter { get; init; }
    
    /// <summary>
    /// Czy to separator (pionowa linia rozdzielająca grupy przycisków)?
    /// Separatory nie są klikalne.
    /// </summary>
    public bool IsSeparator { get; init; }
    
    /// <summary>
    /// Czy wstawić nową linię przed składnią?
    /// True dla elementów blokowych (nagłówki, listy, cytaty).
    /// </summary>
    public bool NewLineBefore { get; init; }
    
    /// <summary>
    /// Skrót klawiszowy wyświetlany w tooltipie.
    /// Przykłady: "Ctrl+B", "Ctrl+I", "Ctrl+Shift+1"
    /// </summary>
    public string? Shortcut { get; init; }
    
    /// <summary>
    /// Statyczna właściwość tworząca separator.
    /// </summary>
    public static ToolbarItem Separator => new()
    {
        Id = "separator",
        Icon = "",
        TooltipKey = "",
        IsSeparator = true
    };
}
```

### ToolbarItems (predefiniowane)

Klasa statyczna z gotowymi definicjami przycisków.

```csharp
public static class ToolbarItems
{
    // Formatowanie tekstu
    public static ToolbarItem Bold => new() { Id = "bold", Icon = "format_bold", TooltipKey = "bold", MarkdownBefore = "**", MarkdownAfter = "**", Shortcut = "Ctrl+B" };
    public static ToolbarItem Italic => new() { Id = "italic", Icon = "format_italic", TooltipKey = "italic", MarkdownBefore = "*", MarkdownAfter = "*", Shortcut = "Ctrl+I" };
    public static ToolbarItem Strikethrough => new() { Id = "strikethrough", Icon = "strikethrough_s", TooltipKey = "strikethrough", MarkdownBefore = "~~", MarkdownAfter = "~~" };
    
    // Nagłówki
    public static ToolbarItem Heading1 => new() { Id = "heading1", Icon = "title", TooltipKey = "heading1", MarkdownBefore = "# ", NewLineBefore = true };
    public static ToolbarItem Heading2 => new() { Id = "heading2", Icon = "title", TooltipKey = "heading2", MarkdownBefore = "## ", NewLineBefore = true };
    public static ToolbarItem Heading3 => new() { Id = "heading3", Icon = "title", TooltipKey = "heading3", MarkdownBefore = "### ", NewLineBefore = true };
    
    // Listy
    public static ToolbarItem BulletList => new() { Id = "bulletList", Icon = "format_list_bulleted", TooltipKey = "bulletList", MarkdownBefore = "- ", NewLineBefore = true };
    public static ToolbarItem NumberedList => new() { Id = "numberedList", Icon = "format_list_numbered", TooltipKey = "numberedList", MarkdownBefore = "1. ", NewLineBefore = true };
    
    // Bloki
    public static ToolbarItem Quote => new() { Id = "quote", Icon = "format_quote", TooltipKey = "quote", MarkdownBefore = "> ", NewLineBefore = true };
    public static ToolbarItem Code => new() { Id = "code", Icon = "code", TooltipKey = "code", MarkdownBefore = "`", MarkdownAfter = "`" };
    public static ToolbarItem CodeBlock => new() { Id = "codeBlock", Icon = "code_blocks", TooltipKey = "codeBlock", MarkdownBefore = "```\n", MarkdownAfter = "\n```", NewLineBefore = true };
    
    // Wstawianie
    public static ToolbarItem Link => new() { Id = "link", Icon = "link", TooltipKey = "link", MarkdownBefore = "[", MarkdownAfter = "](url)" };
    public static ToolbarItem Image => new() { Id = "image", Icon = "image", TooltipKey = "image" };
    public static ToolbarItem Table => new() { Id = "table", Icon = "table_chart", TooltipKey = "table" };
    public static ToolbarItem HorizontalRule => new() { Id = "horizontalRule", Icon = "horizontal_rule", TooltipKey = "horizontalRule", MarkdownBefore = "\n---\n", NewLineBefore = true };
    
    // Akcje
    public static ToolbarItem Undo => new() { Id = "undo", Icon = "undo", TooltipKey = "undo", Shortcut = "Ctrl+Z" };
    public static ToolbarItem Redo => new() { Id = "redo", Icon = "redo", TooltipKey = "redo", Shortcut = "Ctrl+Y" };
    public static ToolbarItem Preview => new() { Id = "preview", Icon = "visibility", TooltipKey = "preview" };
    public static ToolbarItem Fullscreen => new() { Id = "fullscreen", Icon = "fullscreen", TooltipKey = "fullscreen" };
    public static ToolbarItem SwitchMode => new() { Id = "switchMode", Icon = "swap_horiz", TooltipKey = "switchMode" };
    
    // Domyślny zestaw
    public static IReadOnlyList<ToolbarItem> Default => new[] { /* wszystkie powyższe */ };
}
```

#### Przykład: Własny uproszczony pasek narzędzi

```csharp
// Tylko podstawowe formatowanie - dla prostego komentarza
private static readonly IReadOnlyList<ToolbarItem> SimpleToolbar = new[]
{
    ToolbarItems.Bold,
    ToolbarItems.Italic,
    ToolbarItem.Separator,
    ToolbarItems.Link,
    ToolbarItems.Image
};

// Pełny pasek dla tworzenia artykułów
private static readonly IReadOnlyList<ToolbarItem> ArticleToolbar = new[]
{
    ToolbarItems.Undo,
    ToolbarItems.Redo,
    ToolbarItem.Separator,
    ToolbarItems.Bold,
    ToolbarItems.Italic,
    ToolbarItems.Strikethrough,
    ToolbarItem.Separator,
    ToolbarItems.Heading1,
    ToolbarItems.Heading2,
    ToolbarItems.Heading3,
    ToolbarItem.Separator,
    ToolbarItems.BulletList,
    ToolbarItems.NumberedList,
    ToolbarItems.Quote,
    ToolbarItem.Separator,
    ToolbarItems.Code,
    ToolbarItems.CodeBlock,
    ToolbarItem.Separator,
    ToolbarItems.Link,
    ToolbarItems.Image,
    ToolbarItems.Table,
    ToolbarItems.HorizontalRule,
    ToolbarItem.Separator,
    ToolbarItems.Preview,
    ToolbarItems.Fullscreen,
    ToolbarItems.SwitchMode
};

// Własny przycisk z niestandardową akcją
private static readonly ToolbarItem CustomEmojiButton = new()
{
    Id = "insertEmoji",
    Icon = "emoji_emotions",
    TooltipKey = "insertEmoji",
    MarkdownBefore = "😀"
};
```

---

## Architektura JavaScript

Edytor wymaga minimalnej ilości JavaScript do operacji na DOM, których Blazor nie obsługuje natywnie (kursor w textarea, `document.execCommand`, drag resize obrazków).

### Struktura plików

| Plik | Opis |
|------|------|
| `wwwroot/js/ste-interop.js` | Zunifikowany moduł ES — jedyny plik JS w projekcie |
| `Services/SteJsInterop.cs` | C# wrapper — jedyny plik interop C# |

### Moduł `ste-interop.js`

Podzielony na 5 sekcji:

1. **Operacje textarea** (tryb Markdown) — `getSelection`, `setSelection`, `insertText`, `getCurrentLine`, `syncScroll`
2. **Operacje WYSIWYG** (contenteditable) — `execCommand`, `getHtml`, `setHtml`, `insertHtml`, `alignText`, `formatBlock`, `indent`, `outdent`
3. **Skróty klawiaturowe WYSIWYG** — `initKeyboardShortcuts`, `disposeKeyboardShortcuts` (Ctrl+B/I/U/K/Z/Y)
4. **Drag & drop i wklejanie obrazków** — `initImageDragDrop`, `disposeImageDragDrop`
5. **Resize obrazków** — `initImageResize`, `disposeImageResize`, `setSelectedImageSize`, `deselectImage`

Moduł jest ładowany automatycznie przez `SteJsInterop` przy użyciu dynamicznego `import()`. **Nie trzeba dodawać `<script>` tagu** — wystarczy referencja na CSS.

### `SteJsInterop.cs`

```csharp
// Tworzenie instancji (w OnInitialized)
private SteJsInterop? _jsInterop;
_jsInterop = new SteJsInterop(JSRuntime);

// Użycie (w OnAfterRenderAsync lub handlerach)
await _jsInterop.ExecCommandAsync("bold");
await _jsInterop.InsertTextAsync(textarea, "**", "**", false);
await _jsInterop.InitImageResizeAsync(wysiwygRef, dotNetRef);

// Dispose (w DisposeAsync)
await _jsInterop.DisposeAsync();
```

---

## Zmiana rozmiaru obrazów

W trybie WYSIWYG obrazy można interaktywnie zmieniać rozmiar na dwa sposoby:

### Drag resize (przeciąganie za rogi)

1. Kliknij obraz → pojawi się niebieska ramka z uchwytami na rogach i etykietą rozmiaru
2. Przeciągnij dowolny uchwyt → obraz zmienia rozmiar z zachowaniem proporcji
3. Przytrzymaj **Shift** podczas przeciągania → zmiana rozmiaru bez zachowania proporcji
4. Naciśnij **Escape** → odznacz obraz
5. Naciśnij **Delete** / **Backspace** → usuń zaznaczony obraz

### Popup z wymiarami

1. **Podwójne kliknięcie** na obraz → otwiera popup z polami Szerokość/Wysokość
2. Lub kliknij **etykietę rozmiaru** (np. "640 × 480") pod obrazem
3. Wpisz dokładne wymiary → kliknij "Zastosuj"
4. Checkbox "Zachowaj proporcje" automatycznie przelicza drugi wymiar

### Komponent `ImageResizePopup`

Popup jest komponentem Blazor (`ImageResizePopup.razor`) — nie wymaga dodatkowej konfiguracji. Jest automatycznie renderowany w `RadzenMarkdownEditor` w trybie WYSIWYG.

### Inicjalizacja resize

Resize jest automatycznie inicjalizowany w `OnAfterRenderAsync` i reinicjalizowany przy każdym przełączeniu trybu WYSIWYG ↔ Markdown.

```csharp
// Wewnętrznie w RadzenMarkdownEditor:
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && _currentMode == EditorMode.Wysiwyg)
    {
        await _jsInterop.SetHtmlAsync(_wysiwygRef, html);
        await InitImageResize(); // Automatyczna inicjalizacja
    }
}
```

### Stylizacja

Style dla resize znajdują się w `wwwroot/css/wysiwyg.css` — klasy:
- `.ste-img-selected` — niebieska ramka zaznaczonego obrazu
- `.ste-img-overlay` — kontener uchwytów
- `.ste-img-handle` / `.ste-img-handle-nw/ne/sw/se` — uchwyty na rogach
- `.ste-img-size-label` — etykieta rozmiaru pod obrazem
- `.ste-img-resize-popup` — popup z wymiarami

---

## Pasek statusu

Edytor wyświetla pasek statusu na dole z trzema licznikami aktualizowanymi w czasie rzeczywistym:

| Licznik | Opis |
|---------|------|
| **Słowa** | Liczba słów (rozdzielonych białymi znakami) |
| **Znaki** | Całkowita liczba znaków |
| **Linie** | Liczba linii tekstu |

Pasek statusu jest zawsze widoczny (niezależnie od trybu edycji). Etykiety są tłumaczone przez `ILocalizationProvider` za pomocą kluczy `words`, `characters`, `lines`.

Stylowanie paska za pomocą klasy CSS `.ste-status-bar` — można nadpisać w arkuszu stylów aplikacji.

---

## Skróty klawiaturowe WYSIWYG

W trybie WYSIWYG dostępne są następujące skróty klawiaturowe:

| Skrót | Akcja |
|-------|-------|
| `Ctrl+B` | Pogrubienie |
| `Ctrl+I` | Kursywa |
| `Ctrl+U` | Podkreślenie |
| `Ctrl+K` | Wstaw link (wyświetla prompt z polem URL) |
| `Ctrl+Z` | Cofnij |
| `Ctrl+Y` | Ponów |

Skróty są inicjalizowane automatycznie przy przełączeniu do trybu WYSIWYG i zwalniane przy przełączeniu do trybu Markdown lub przy niszczeniu komponentu.

---

## Drag & drop i wklejanie obrazków (WYSIWYG)

W trybie WYSIWYG można wstawiać obrazy przez:

1. **Przeciągnięcie pliku** (drag & drop) — przeciągnij plik obrazu z systemu plików i upuść na obszar edytora
2. **Wklejenie ze schowka** (Ctrl+V) — wklej obraz skopiowany np. ze zrzutu ekranu lub przeglądarki

Wstawiony obraz jest przekazywany do skonfigurowanego `ImageUploadHandler` (tak samo jak przy wyborze pliku przez pasek narzędzi). Obsługiwane typy MIME to te zdefiniowane w `IImageUploadHandler.AllowedContentTypes`.

**Callback JS → Blazor**: `OnImageDropped(string fileName, string base64, string contentType)` — wywoływany przez JS po upuszczeniu/wklejeniu; waliduje rozmiar i typ, następnie wywołuje `UploadAsync` i wstawia URL do edytora.

---

## Bezpieczeństwo 

SimpleTextEditor zawiera wbudowane mechanizmy bezpieczeństwa chroniące przed najczęstszymi atakami webowymi.

### Trust boundaries

```
Użytkownik (niezaufany)
    │
    ▼
┌─────────────────────────────────┐
│ Przeglądarka (JS interop)      │
│ • Walidacja protokołów URL     │
│ • Blokada javascript:/data:    │
│ • Limit plików w drop/paste    │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│ Blazor Server (C#)             │
│ • Sanitizacja HTML (allowlista)│
│ • Walidacja magic bytes        │
│ • Kontrola rozmiaru base64     │
│ • Debounce z cleanup zasobów   │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│ Przechowywanie (zaufane)       │
│ • Baza danych / S3 / Blob      │
└─────────────────────────────────┘
```

### Sanitizacja HTML (XSS)

Cały HTML generowany przez Markdig oraz HTML wprowadzany w trybie WYSIWYG przechodzi przez sanitizer oparty o **allowlistę**:

| Element | Dozwolone | Blokowane |
|---------|-----------|-----------|
| **Tagi** | `h1-h6`, `p`, `strong`, `em`, `a`, `img`, `table`, `ul`, `ol`, `code`, `pre`, `blockquote`... | `script`, `iframe`, `object`, `embed`, `form`, `style`... |
| **Atrybuty** | `href`, `src`, `alt`, `class`, `id`, `width`, `height`, `style` (z filtrem CSS) | `onclick`, `onerror`, `onload`, `onmouseover`... |
| **Protokoły URL** | `http:`, `https:`, `mailto:`, `tel:` | `javascript:`, `vbscript:` |
| **data: URI** | Tylko `data:image/*` w atrybucie `src` tagu `img` | `data:text/html`, `data:application/...` |

Raw HTML w Markdown jest escapowany (`DisableHtml()` w Markdig), co uniemożliwia wstrzyknięcie kodu przez `<script>` lub `<img onerror=...>`.

### Walidacja uploadu obrazów

Przy użyciu `ImageUploadHandlerBase`:

1. **Walidacja rozmiaru** — plik musi mieścić się w `MaxFileSizeBytes` (domyślnie 10 MB)
2. **Walidacja typu MIME** — tylko dozwolone typy (`image/jpeg`, `image/png`, `image/gif`, `image/webp`)
3. **Walidacja magic bytes** — sygnatura binarna pliku musi zgadzać się z deklarowanym typem MIME (ochrona przed spoofingiem: plik `.html` z `Content-Type: image/png` zostanie odrzucony)
4. **Rozszerzenie z MIME** — nazwa pliku jest generowana na podstawie zweryfikowanego MIME, nie z oryginalnej nazwy od klienta
5. **SVG domyślnie wyłączone** — SVG jest aktywnym formatem (może zawierać `<script>`) i nie jest dozwolony domyślnie

### Ochrona przed DoS (drag & drop)

- Rozmiar base64 jest szacowany **przed dekodowaniem** — zbyt duże payloady są odrzucane bez alokacji pamięci
- `Convert.TryFromBase64String` z kontrolowanym buforem zamiast `Convert.FromBase64String`
- Limit jednoczesnych plików w jednym drop/paste: **10**

### Brak `eval()` w kodzie

Wszystkie operacje JS wykonywane są przez dedykowane, eksportowane funkcje w module `ste-interop.js` 

### Zalecenia dla produkcji

1. **SignalR** — zmniejsz `MaximumReceiveMessageSize` (domyślnie 2 MB w produkcji, 10 MB w dev)
2. **Rate limiting** — dodaj rate limiting na endpointy uploadu obrazów
3. **CSP header** — skonfiguruj Content-Security-Policy bez `unsafe-eval`
4. **HTTPS** — wymuszaj HTTPS w produkcji (`UseHsts()`, `UseHttpsRedirection()`)

---

## Przykłady użycia

### 1. Podstawowy edytor

```razor
<RadzenMarkdownEditor @bind-Value="content" />

@code {
    private string content = "";
}
```

### 2. Edytor z ciemnym motywem i minimalną wysokością

```razor
<RadzenMarkdownEditor 
    @bind-Value="content" 
    Theme="dark"
    MinHeight="500" />
```

### 3. Edytor tylko do odczytu (przeglądarka artykułów)

```razor
<RadzenMarkdownEditor 
    @bind-Value="articleContent" 
    ReadOnly="true"
    Mode="EditorMode.Wysiwyg" />
```

### 4. Edytor z uploadem do Azure Blob

```razor
@inject IImageUploadHandler ImageHandler

<RadzenMarkdownEditor 
    @bind-Value="content" 
    ImageUploadHandler="ImageHandler" />
```

### 5. Edytor z polskim interfejsem

```razor
<RadzenMarkdownEditor 
    @bind-Value="content" 
    LocalizationProvider="@(new PolishLocalizationProvider())" />
```

### 6. Prosty edytor komentarzy (ograniczony toolbar)

```razor
<RadzenMarkdownEditor 
    @bind-Value="comment"
    ToolbarItems="_commentToolbar"
    MinHeight="150"
    MaxHeight="300"
    Placeholder="Dodaj komentarz..." />

@code {
    private string comment = "";
    
    private static readonly IReadOnlyList<ToolbarItem> _commentToolbar = new[]
    {
        ToolbarItems.Bold,
        ToolbarItems.Italic,
        ToolbarItem.Separator,
        ToolbarItems.Link,
        ToolbarItems.Code
    };
}
```

### 7. Edytor artykułów z auto-zapisem

```razor
@inject IArticleService ArticleService

<RadzenMarkdownEditor 
    @bind-Value="article.Content"
    MinHeight="600"
    OnChange="HandleAutoSave" />

<div class="text-muted small mt-2">
    @if (isSaving)
    {
        <span>Zapisywanie...</span>
    }
    else if (lastSaved.HasValue)
    {
        <span>Ostatni zapis: @lastSaved.Value.ToString("HH:mm:ss")</span>
    }
</div>

@code {
    private Article article = new();
    private bool isSaving = false;
    private DateTime? lastSaved;
    private Timer? autoSaveTimer;
    
    private void HandleAutoSave(string content)
    {
        // Debounce - zapisz po 2 sekundach bezczynności
        autoSaveTimer?.Dispose();
        autoSaveTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                isSaving = true;
                StateHasChanged();
                
                await ArticleService.SaveDraftAsync(article);
                
                isSaving = false;
                lastSaved = DateTime.Now;
                StateHasChanged();
            });
        }, null, 2000, Timeout.Infinite);
    }
}
```

### 8. Formularz tworzenia artykułu z walidacją

```razor
@inject NavigationManager Navigation
@inject IArticleService ArticleService

<EditForm Model="article" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <div class="mb-3">
        <label class="form-label">Tytuł</label>
        <InputText @bind-Value="article.Title" class="form-control" />
        <ValidationMessage For="() => article.Title" />
    </div>
    
    <div class="mb-3">
        <label class="form-label">Kategoria</label>
        <InputSelect @bind-Value="article.CategoryId" class="form-control">
            <option value="">-- Wybierz kategorię --</option>
            @foreach (var cat in categories)
            {
                <option value="@cat.Id">@cat.Name</option>
            }
        </InputSelect>
        <ValidationMessage For="() => article.CategoryId" />
    </div>
    
    <div class="mb-3">
        <label class="form-label">Treść</label>
        <RadzenMarkdownEditor 
            @bind-Value="article.Content" 
            MinHeight="500"
            ImageUploadHandler="imageHandler"
            LocalizationProvider="localizationProvider" />
        <ValidationMessage For="() => article.Content" />
    </div>
    
    <div class="mb-3">
        <label class="form-label">Tagi</label>
        <InputText @bind-Value="article.Tags" class="form-control" placeholder="tag1, tag2, tag3" />
    </div>
    
    <div class="d-flex gap-2">
        <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
            @if (isSubmitting)
            {
                <span class="spinner-border spinner-border-sm me-2"></span>
            }
            Opublikuj
        </button>
        <button type="button" class="btn btn-secondary" @onclick="SaveDraft">
            Zapisz jako szkic
        </button>
    </div>
</EditForm>

@code {
    private ArticleModel article = new();
    private List<Category> categories = new();
    private bool isSubmitting = false;
    
    private IImageUploadHandler imageHandler = new AzureBlobImageHandler(...);
    private ILocalizationProvider localizationProvider = new PolishLocalizationProvider();
    
    protected override async Task OnInitializedAsync()
    {
        categories = await ArticleService.GetCategoriesAsync();
    }
    
    private async Task HandleSubmit()
    {
        isSubmitting = true;
        article.Status = ArticleStatus.Published;
        await ArticleService.CreateAsync(article);
        Navigation.NavigateTo($"/articles/{article.Slug}");
    }
    
    private async Task SaveDraft()
    {
        article.Status = ArticleStatus.Draft;
        await ArticleService.SaveDraftAsync(article);
    }
}
```

### 9. Edytor z responsywnym podglądem (mobile-friendly)

```razor
<div class="editor-wrapper">
    <RadzenMarkdownEditor 
        @bind-Value="content"
        Mode="EditorMode.Markdown"
        PreviewMode="@currentPreviewMode"
        MinHeight="@GetMinHeight()" />
</div>

@code {
    private string content = "";
    
    [Inject] private IJSRuntime JS { get; set; } = default!;
    
    private PreviewMode currentPreviewMode = PreviewMode.SideBySide;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Użyj dedykowanej funkcji JS zamiast eval
            var width = await JS.InvokeAsync<int>("getWindowWidth");
            currentPreviewMode = width < 768 ? PreviewMode.Toggle : PreviewMode.SideBySide;
            StateHasChanged();
        }
    }
    
    private int GetMinHeight() => currentPreviewMode == PreviewMode.Toggle ? 400 : 500;
}
```

### 10. Edytor z podglądem w czasie rzeczywistym (osobny panel)

```razor
<div class="row">
    <div class="col-md-6">
        <h4>Edytor</h4>
        <RadzenMarkdownEditor 
            @bind-Value="content"
            Mode="EditorMode.Markdown"
            PreviewMode="PreviewMode.None"
            MinHeight="600" />
    </div>
    <div class="col-md-6">
        <h4>Podgląd w czasie rzeczywistym</h4>
        <div class="preview-panel border rounded p-3" style="min-height: 600px;">
            @((MarkupString)htmlPreview)
        </div>
    </div>
</div>

@code {
    private string content = "";
    private string htmlPreview = "";
    
    [Inject] private IMarkdownParser MarkdownParser { get; set; } = default!;
    
    protected override void OnParametersSet()
    {
        htmlPreview = MarkdownParser.ToHtml(content);
    }
}
```

---

## Wsparcie

Jeśli masz pytania lub problemy, utwórz issue w repozytorium projektu.
