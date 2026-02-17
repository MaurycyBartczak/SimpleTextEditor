using SimpleTextEditor.Core.Services;

namespace SimpleTextEditor.Core.Tests.Services;

public class ImageUploadHandlerBaseTests
{
    /// <summary>
    /// Testowa implementacja — zwraca URL z nazwą pliku.
    /// </summary>
    private class TestImageHandler : ImageUploadHandlerBase
    {
        public string? LastSavedFileName { get; private set; }
        public byte[]? LastSavedContent { get; private set; }
        public string? LastSavedContentType { get; private set; }

        protected override Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
        {
            LastSavedFileName = uniqueFileName;
            LastSavedContent = content;
            LastSavedContentType = contentType;
            return Task.FromResult($"https://storage.test/{uniqueFileName}");
        }
    }

    private class SmallLimitHandler : ImageUploadHandlerBase
    {
        public override long MaxFileSizeBytes => 100;

        protected override Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
            => Task.FromResult($"https://storage.test/{uniqueFileName}");
    }

    private class RestrictedTypeHandler : ImageUploadHandlerBase
    {
        public override IReadOnlyList<string> AllowedContentTypes => new[] { "image/png" };

        protected override Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
            => Task.FromResult($"https://storage.test/{uniqueFileName}");
    }

    // Walidne sygnatury plików do testów
    private static readonly byte[] ValidPng = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] ValidJpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
    private static readonly byte[] ValidGif = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
    private static readonly byte[] ValidWebP = { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };

    [Fact]
    public async Task UploadAsync_ValidPng_CallsSaveAsync()
    {
        var handler = new TestImageHandler();

        var result = await handler.UploadAsync("photo.png", ValidPng, "image/png");

        Assert.NotNull(handler.LastSavedFileName);
        Assert.EndsWith(".png", handler.LastSavedFileName);
        Assert.Equal(ValidPng, handler.LastSavedContent);
        Assert.Equal("image/png", handler.LastSavedContentType);
        Assert.StartsWith("https://storage.test/", result);
    }

    [Fact]
    public async Task UploadAsync_FileTooLarge_ThrowsException()
    {
        var handler = new SmallLimitHandler();
        var content = new byte[200]; // Over 100 byte limit

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.UploadAsync("big.png", content, "image/png"));

        Assert.Contains("przekracza maksymalny", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_InvalidContentType_ThrowsException()
    {
        var handler = new RestrictedTypeHandler();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.UploadAsync("photo.jpg", ValidJpeg, "image/jpeg"));

        Assert.Contains("niedozwolony", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_GeneratesExtensionFromMime_NotFromFilename()
    {
        var handler = new TestImageHandler();

        // Plik z rozszerzeniem .bmp ale walidnym PNG i typem image/png
        await handler.UploadAsync("sneaky.bmp", ValidPng, "image/png");

        Assert.NotNull(handler.LastSavedFileName);
        Assert.EndsWith(".png", handler.LastSavedFileName);
        Assert.DoesNotContain(".bmp", handler.LastSavedFileName);
    }

    [Fact]
    public async Task UploadAsync_GeneratesUniqueNamesEachTime()
    {
        var handler = new TestImageHandler();

        await handler.UploadAsync("a.png", ValidPng, "image/png");
        var name1 = handler.LastSavedFileName;

        await handler.UploadAsync("a.png", ValidPng, "image/png");
        var name2 = handler.LastSavedFileName;

        Assert.NotEqual(name1, name2);
    }

    [Fact]
    public void DefaultMaxFileSizeBytes_Is10MB()
    {
        var handler = new TestImageHandler();
        Assert.Equal(10 * 1024 * 1024, handler.MaxFileSizeBytes);
    }

    [Fact]
    public void DefaultAllowedContentTypes_Contains4Types()
    {
        var handler = new TestImageHandler();
        Assert.Equal(4, handler.AllowedContentTypes.Count);
    }

    [Fact]
    public void DefaultAllowedTypes_DoesNotContainSvg()
    {
        var handler = new TestImageHandler();
        Assert.DoesNotContain("image/svg+xml", handler.AllowedContentTypes);
    }

    // === Testy magic bytes ===

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, "image/jpeg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, "image/png")]
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38 }, "image/gif")]
    public void DetectMime_ValidSignature_ReturnsCorrectMime(byte[] header, string expectedMime)
    {
        var result = ImageUploadHandlerBase.DetectMimeFromMagicBytes(header);
        Assert.Equal(expectedMime, result);
    }

    [Fact]
    public void DetectMime_WebP_ReturnsCorrectMime()
    {
        var result = ImageUploadHandlerBase.DetectMimeFromMagicBytes(ValidWebP);
        Assert.Equal("image/webp", result);
    }

    [Fact]
    public void DetectMime_UnknownBytes_ReturnsNull()
    {
        var result = ImageUploadHandlerBase.DetectMimeFromMagicBytes(new byte[] { 0x00, 0x00, 0x00, 0x00 });
        Assert.Null(result);
    }

    [Fact]
    public void DetectMime_TooShort_ReturnsNull()
    {
        var result = ImageUploadHandlerBase.DetectMimeFromMagicBytes(new byte[] { 0xFF, 0xD8 });
        Assert.Null(result);
    }

    // === Testy spoofingu ===

    [Fact]
    public async Task UploadAsync_SpoofedMime_HtmlAsPng_Throws()
    {
        var handler = new TestImageHandler();
        var htmlContent = "<html><body>evil</body></html>"u8.ToArray();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.UploadAsync("evil.html", htmlContent, "image/png"));

        Assert.Contains("Sygnatura pliku nie zgadza się", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_SpoofedMime_JpegAsPng_Throws()
    {
        var handler = new TestImageHandler();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.UploadAsync("image.png", ValidJpeg, "image/png"));

        Assert.Contains("Sygnatura pliku nie zgadza się", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_ValidJpeg_Succeeds()
    {
        var handler = new TestImageHandler();
        var result = await handler.UploadAsync("photo.jpg", ValidJpeg, "image/jpeg");
        Assert.EndsWith(".jpg", handler.LastSavedFileName!);
    }

    [Fact]
    public async Task UploadAsync_ValidGif_Succeeds()
    {
        var handler = new TestImageHandler();
        var result = await handler.UploadAsync("anim.gif", ValidGif, "image/gif");
        Assert.EndsWith(".gif", handler.LastSavedFileName!);
    }

    [Fact]
    public async Task UploadAsync_ValidWebP_Succeeds()
    {
        var handler = new TestImageHandler();
        var result = await handler.UploadAsync("photo.webp", ValidWebP, "image/webp");
        Assert.EndsWith(".webp", handler.LastSavedFileName!);
    }
}
