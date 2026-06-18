using System.Globalization;
using System.Text;
using Dhole.Storage.Application.Abstractions.Naming;

namespace Dhole.Storage.Application.Naming;

public sealed class StorageFileNameGenerator : IStorageFileNameGenerator
{
    public string GenerateStoredFileName(string originalFileName, string? extension = null)
    {
        var safeName = BuildSafeName(originalFileName);
        var safeExtension = BuildExtension(originalFileName, extension);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

        return $"{timestamp}-{Guid.NewGuid():N}-{safeName}{safeExtension}";
    }

    public string GenerateVersionedStoredFileName(
        Guid fileId,
        int versionNumber,
        string originalFileName,
        string? extension = null
    )
    {
        var safeName = BuildSafeName(originalFileName);
        var safeExtension = BuildExtension(originalFileName, extension);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

        return $"{fileId:N}-v{versionNumber}-{timestamp}-{safeName}{safeExtension}";
    }

    private static string BuildSafeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "file";
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(value);

        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            return "file";
        }

        var normalized = RemoveDiacritics(fileNameWithoutExtension.Trim()).ToLowerInvariant();

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            if (character is '-' or '_' or '.')
            {
                builder.Append(character);
                continue;
            }

            builder.Append('-');
        }

        var result = builder.ToString().Trim('-');

        while (result.Contains("--", StringComparison.Ordinal))
        {
            result = result.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(result) ? "file" : result;
    }

    private static string BuildExtension(string originalFileName, string? extension)
    {
        var value = string.IsNullOrWhiteSpace(extension)
            ? Path.GetExtension(originalFileName)
            : extension;

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = value.Trim().ToLowerInvariant();

        return value.StartsWith('.'.ToString(), StringComparison.Ordinal) ? value : $".{value}";
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
