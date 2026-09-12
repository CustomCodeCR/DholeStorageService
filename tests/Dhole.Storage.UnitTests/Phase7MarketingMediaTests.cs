using Dhole.Storage.Application.Abstractions.Media;
using Dhole.Storage.Application.Media;
using Dhole.Storage.Infrastructure.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StorageFile = Dhole.Storage.Domain.Files.Entities.File;

namespace Dhole.Storage.UnitTests;

[TestClass]
public sealed class Phase7MarketingMediaTests
{
    private static readonly MarketingMediaLimits Limits = new(
        25 * 1024 * 1024,
        100 * 1024 * 1024,
        50 * 1024 * 1024);

    [TestMethod]
    public void Validate_AcceptsPdfWithMatchingMimeExtensionAndSignature()
    {
        var bytes = "%PDF-1.7 sample"u8.ToArray();

        var result = MarketingMediaPolicy.Validate(
            "brochure.pdf",
            "application/pdf",
            bytes.Length,
            bytes,
            Limits);

        Assert.AreEqual(MarketingMediaCategory.Pdf, result.Category);
        Assert.AreEqual(".pdf", result.Extension);
    }

    [TestMethod]
    public void Validate_RejectsMimeThatDoesNotMatchExtension()
    {
        var bytes = "%PDF-1.7 sample"u8.ToArray();

        Assert.ThrowsExactly<InvalidOperationException>(() => MarketingMediaPolicy.Validate(
            "brochure.pdf",
            "image/png",
            bytes.Length,
            bytes,
            Limits));
    }

    [TestMethod]
    public void Validate_RejectsSpoofedFileSignature()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        Assert.ThrowsExactly<InvalidOperationException>(() => MarketingMediaPolicy.Validate(
            "photo.png",
            "image/png",
            bytes.Length,
            bytes,
            Limits));
    }

    [TestMethod]
    public void Validate_RejectsFilesOverCategoryLimit()
    {
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
        var tinyLimits = new MarketingMediaLimits(3, 100, 100);

        Assert.ThrowsExactly<InvalidOperationException>(() => MarketingMediaPolicy.Validate(
            "photo.jpg",
            "image/jpeg",
            bytes.Length,
            bytes,
            tinyLimits));
    }

    [TestMethod]
    public async Task ImageProcessor_GeneratesThumbnailAndOptimizedWebp()
    {
        using var image = new Image<Rgba32>(1200, 800);
        await using var source = new MemoryStream();
        await image.SaveAsPngAsync(source);

        var processor = new MarketingMediaProcessor();
        var result = await processor.ProcessAsync(new MarketingMediaInput(
            "hero.png",
            "image/png",
            source.ToArray()));

        Assert.AreEqual("image", result.Category);
        Assert.AreEqual(1200, result.Width);
        Assert.AreEqual(800, result.Height);
        Assert.AreEqual(2, result.Assets.Count);
        Assert.IsTrue(result.Assets.Any(x => x.Key == "thumbnail" && x.ContentType == "image/webp"));
        Assert.IsTrue(result.Assets.Any(x => x.Key == "optimized" && x.ContentType == "image/webp"));
        Assert.IsTrue(result.Assets.All(x => x.Content.Length > 0));
    }

    [TestMethod]
    public void File_CanPersistGeneratedMarketingMetadata()
    {
        var file = StorageFile.Upload(
            Guid.NewGuid(),
            "DholeContentService",
            "CmsMedia",
            Guid.NewGuid(),
            "hero.jpg",
            "hero-stored.jpg",
            "image/jpeg",
            ".jpg",
            100,
            "cms/hero-stored.jpg",
            "checksum",
            null,
            Guid.NewGuid());

        file.UpdateMetadata("{\"marketingMedia\":{\"category\":\"image\"}}", Guid.NewGuid());

        Assert.IsNotNull(file.MetadataJson);
        StringAssert.Contains(file.MetadataJson, "marketingMedia");
    }
}
