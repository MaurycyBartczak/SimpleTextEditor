using SimpleTextEditor.Core.Services;

namespace SimpleTextEditor.Core.Tests.Services;

public class ImageUploadHandlerBaseTests
{
    /// <summary>
    /// Concrete test implementation that just returns a fixed URL.
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
        public override long MaxFileSizeBytes => 100; // 100 bytes

        protected override Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
            => Task.FromResult($"https://storage.test/{uniqueFileName}");
    }

    private class RestrictedTypeHandler : ImageUploadHandlerBase
    {
        public override IReadOnlyList<string> AllowedContentTypes => new[] { "image/png" };

        protected override Task<string> SaveAsync(string uniqueFileName, byte[] content, string contentType)
            => Task.FromResult($"https://storage.test/{uniqueFileName}");
    }

    [Fact]
    public async Task UploadAsync_ValidFile_CallsSaveAsync()
    {
        var handler = new TestImageHandler();
        var content = new byte[] { 1, 2, 3 };

        var result = await handler.UploadAsync("photo.png", content, "image/png");

        Assert.NotNull(handler.LastSavedFileName);
        Assert.EndsWith(".png", handler.LastSavedFileName);
        Assert.Equal(content, handler.LastSavedContent);
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
        var content = new byte[] { 1, 2, 3 };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.UploadAsync("photo.jpg", content, "image/jpeg"));
        
        Assert.Contains("niedozwolony", ex.Message);
    }

    [Fact]
    public async Task GenerateUniqueFileName_PreservesExtension()
    {
        var handler = new TestImageHandler();
        
        await handler.UploadAsync("my-photo.jpeg", new byte[] { 1 }, "image/jpeg");

        Assert.NotNull(handler.LastSavedFileName);
        Assert.EndsWith(".jpeg", handler.LastSavedFileName);
        // Should be a GUID + extension
        Assert.True(Guid.TryParse(
            Path.GetFileNameWithoutExtension(handler.LastSavedFileName), out _));
    }

    [Fact]
    public async Task GenerateUniqueFileName_NoExtension_UsesMimeMapping()
    {
        var handler = new TestImageHandler();
        
        await handler.UploadAsync("noext", new byte[] { 1 }, "image/webp");

        Assert.NotNull(handler.LastSavedFileName);
        Assert.EndsWith(".webp", handler.LastSavedFileName);
    }

    [Fact]
    public async Task UploadAsync_GeneratesUniqueNamesEachTime()
    {
        var handler = new TestImageHandler();

        await handler.UploadAsync("a.png", new byte[] { 1 }, "image/png");
        var name1 = handler.LastSavedFileName;
        
        await handler.UploadAsync("a.png", new byte[] { 1 }, "image/png");
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
    public void DefaultAllowedContentTypes_Contains5Types()
    {
        var handler = new TestImageHandler();
        Assert.Equal(5, handler.AllowedContentTypes.Count);
    }
}
