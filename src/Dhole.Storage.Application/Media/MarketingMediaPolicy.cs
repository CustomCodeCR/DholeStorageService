namespace Dhole.Storage.Application.Media;

public enum MarketingMediaCategory
{
    Image,
    Video,
    Pdf,
    Document
}

public sealed record MarketingMediaLimits(
    long MaximumImageSizeBytes,
    long MaximumVideoSizeBytes,
    long MaximumDocumentSizeBytes);

public sealed record MarketingMediaDescriptor(
    MarketingMediaCategory Category,
    string Extension,
    string ContentType,
    long SizeInBytes);

public static class MarketingMediaPolicy
{
    private static readonly IReadOnlyDictionary<string, string[]> MimeTypesByExtension =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"],
            [".webp"] = ["image/webp"],
            [".avif"] = ["image/avif"],
            [".mp4"] = ["video/mp4"],
            [".webm"] = ["video/webm"],
            [".mov"] = ["video/quicktime"],
            [".pdf"] = ["application/pdf"],
            [".doc"] = ["application/msword", "application/octet-stream"],
            [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/zip", "application/octet-stream"],
            [".xls"] = ["application/vnd.ms-excel", "application/octet-stream"],
            [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/zip", "application/octet-stream"],
            [".ppt"] = ["application/vnd.ms-powerpoint", "application/octet-stream"],
            [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation", "application/zip", "application/octet-stream"],
            [".csv"] = ["text/csv", "text/plain", "application/csv", "application/vnd.ms-excel"],
            [".txt"] = ["text/plain"],
        };

    public static MarketingMediaDescriptor Validate(
        string fileName,
        string? contentType,
        long sizeInBytes,
        ReadOnlySpan<byte> content,
        MarketingMediaLimits limits)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("El nombre del archivo es requerido.");
        if (sizeInBytes <= 0)
            throw new InvalidOperationException("El archivo debe contener datos.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!MimeTypesByExtension.TryGetValue(extension, out var allowedMimeTypes))
            throw new InvalidOperationException($"La extensión '{extension}' no está permitida para multimedia de Mercadeo.");

        var normalizedContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim().ToLowerInvariant();
        if (!allowedMimeTypes.Contains(normalizedContentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"El MIME type '{normalizedContentType}' no coincide con la extensión '{extension}'.");

        var category = Classify(extension);
        var maximum = category switch
        {
            MarketingMediaCategory.Image => limits.MaximumImageSizeBytes,
            MarketingMediaCategory.Video => limits.MaximumVideoSizeBytes,
            _ => limits.MaximumDocumentSizeBytes,
        };
        if (sizeInBytes > maximum)
            throw new InvalidOperationException(
                $"El archivo excede el tamaño máximo permitido para {category}: {maximum} bytes.");

        if (!SignatureMatches(extension, content))
            throw new InvalidOperationException(
                $"El contenido real del archivo no coincide con la extensión '{extension}'.");

        return new MarketingMediaDescriptor(category, extension, normalizedContentType, sizeInBytes);
    }

    private static MarketingMediaCategory Classify(string extension) => extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".webp" or ".avif" => MarketingMediaCategory.Image,
        ".mp4" or ".webm" or ".mov" => MarketingMediaCategory.Video,
        ".pdf" => MarketingMediaCategory.Pdf,
        _ => MarketingMediaCategory.Document,
    };

    private static bool SignatureMatches(string extension, ReadOnlySpan<byte> data)
    {
        if (extension is ".csv" or ".txt") return true;
        if (data.Length < 4) return false;

        return extension switch
        {
            ".jpg" or ".jpeg" => data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            ".png" => data.Length >= 8 && data[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => data.Length >= 12 && IsAscii(data, 0, "RIFF") && IsAscii(data, 8, "WEBP"),
            ".avif" => data.Length >= 12 && IsAscii(data, 4, "ftyp") && (IsAscii(data, 8, "avif") || IsAscii(data, 8, "avis")),
            ".mp4" or ".mov" => data.Length >= 12 && IsAscii(data, 4, "ftyp"),
            ".webm" => data[0] == 0x1A && data[1] == 0x45 && data[2] == 0xDF && data[3] == 0xA3,
            ".pdf" => IsAscii(data, 0, "%PDF"),
            ".docx" or ".xlsx" or ".pptx" => data[0] == 0x50 && data[1] == 0x4B,
            ".doc" or ".xls" or ".ppt" => data.Length >= 8 && data[..8].SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }),
            _ => false,
        };
    }

    private static bool IsAscii(ReadOnlySpan<byte> data, int offset, string value)
    {
        if (data.Length < offset + value.Length) return false;
        for (var i = 0; i < value.Length; i++)
            if (data[offset + i] != (byte)value[i]) return false;
        return true;
    }
}
