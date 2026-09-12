using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Dhole.Storage.Application.Abstractions.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Dhole.Storage.Infrastructure.Media;

public sealed class MarketingMediaProcessor : IMarketingMediaProcessor
{
    public async Task<MarketingMediaProcessingResult> ProcessAsync(
        MarketingMediaInput input,
        CancellationToken cancellationToken = default)
    {
        var contentType = input.ContentType.Trim().ToLowerInvariant();
        if (contentType.StartsWith("image/", StringComparison.Ordinal))
            return await ProcessImageAsync(input, cancellationToken);
        if (contentType.StartsWith("video/", StringComparison.Ordinal))
            return await ProcessVideoAsync(input, cancellationToken);

        var extension = Path.GetExtension(input.FileName).TrimStart('.').ToLowerInvariant();
        return new MarketingMediaProcessingResult(
            contentType == "application/pdf" ? "pdf" : "document",
            extension,
            null,
            null,
            null,
            []);
    }

    private static async Task<MarketingMediaProcessingResult> ProcessImageAsync(
        MarketingMediaInput input,
        CancellationToken cancellationToken)
    {
        using var image = Image.Load(input.Content);
        var format = image.Metadata.DecodedImageFormat?.Name?.ToLowerInvariant()
            ?? Path.GetExtension(input.FileName).TrimStart('.').ToLowerInvariant();

        var assets = new List<GeneratedMediaAsset>(2);
        using (var thumbnail = image.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(480, 480)
        })))
        {
            await using var stream = new MemoryStream();
            await thumbnail.SaveAsWebpAsync(stream, new WebpEncoder { Quality = 76 }, cancellationToken);
            assets.Add(new GeneratedMediaAsset(
                "thumbnail",
                "thumbnail.webp",
                "image/webp",
                stream.ToArray()));
        }

        using (var optimized = image.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(1920, 1920)
        })))
        {
            await using var stream = new MemoryStream();
            await optimized.SaveAsWebpAsync(stream, new WebpEncoder { Quality = 84 }, cancellationToken);
            assets.Add(new GeneratedMediaAsset(
                "optimized",
                "optimized.webp",
                "image/webp",
                stream.ToArray()));
        }

        return new MarketingMediaProcessingResult(
            "image",
            format,
            image.Width,
            image.Height,
            null,
            assets);
    }

    private static async Task<MarketingMediaProcessingResult> ProcessVideoAsync(
        MarketingMediaInput input,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(input.FileName);
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"dhole-media-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        var inputPath = Path.Combine(temporaryDirectory, $"input{extension}");
        var posterPath = Path.Combine(temporaryDirectory, "poster.jpg");

        try
        {
            await File.WriteAllBytesAsync(inputPath, input.Content, cancellationToken);
            var probeJson = await RunProcessAsync(
                "ffprobe",
                [
                    "-v", "error",
                    "-select_streams", "v:0",
                    "-show_entries", "stream=width,height,codec_name,duration:format=duration,format_name",
                    "-of", "json",
                    inputPath
                ],
                cancellationToken);

            using var document = JsonDocument.Parse(probeJson);
            var root = document.RootElement;
            var stream = root.TryGetProperty("streams", out var streams)
                && streams.ValueKind == JsonValueKind.Array
                && streams.GetArrayLength() > 0
                ? streams[0]
                : default;
            var formatNode = root.TryGetProperty("format", out var format) ? format : default;

            var width = ReadInt(stream, "width");
            var height = ReadInt(stream, "height");
            var duration = ReadDouble(stream, "duration") ?? ReadDouble(formatNode, "duration");
            var detectedFormat = ReadString(formatNode, "format_name")
                ?? ReadString(stream, "codec_name")
                ?? extension.TrimStart('.').ToLowerInvariant();

            var assets = new List<GeneratedMediaAsset>();
            try
            {
                await RunProcessAsync(
                    "ffmpeg",
                    [
                        "-y",
                        "-ss", duration is > 2 ? "1" : "0",
                        "-i", inputPath,
                        "-frames:v", "1",
                        "-vf", "scale='min(1280,iw)':-2",
                        "-q:v", "3",
                        posterPath
                    ],
                    cancellationToken);

                if (File.Exists(posterPath))
                {
                    assets.Add(new GeneratedMediaAsset(
                        "poster",
                        "poster.jpg",
                        "image/jpeg",
                        await File.ReadAllBytesAsync(posterPath, cancellationToken)));
                }
            }
            catch (InvalidOperationException)
            {
                // Metadata remains useful even if a source video cannot produce a poster frame.
            }

            return new MarketingMediaProcessingResult(
                "video",
                detectedFormat,
                width,
                height,
                duration,
                assets);
        }
        finally
        {
            try { Directory.Delete(temporaryDirectory, recursive: true); } catch { }
        }
    }

    private static async Task<string> RunProcessAsync(
        string executable,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(executable)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"No se pudo iniciar {executable}.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{executable} falló: {stderr}");
        return stdout;
    }

    private static string? ReadString(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object
           && element.TryGetProperty(property, out var value)
           && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? ReadInt(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object
           && element.TryGetProperty(property, out var value)
           && value.TryGetInt32(out var result)
            ? result
            : null;

    private static double? ReadDouble(JsonElement element, string property)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var value))
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)) return number;
        if (value.ValueKind == JsonValueKind.String
            && double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            return number;
        return null;
    }
}
