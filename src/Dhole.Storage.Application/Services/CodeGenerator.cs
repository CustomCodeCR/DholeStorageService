using System.Globalization;
using System.Text;
using Dhole.Storage.Application.Abstractions.Services;

namespace Dhole.Storage.Application.Services;

public sealed class CodeGenerator : ICodeGenerator
{
    public Task<string> GenerateProviderCodeAsync(
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    )
    {
        return GenerateSequentialAsync("STO", 4, existsAsync, cancellationToken);
    }

    private static async Task<string> GenerateSequentialAsync(
        string prefix,
        int consecutiveLength,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken
    )
    {
        var year = DateTime.UtcNow.Year;

        for (var number = 1; number <= 9999; number++)
        {
            var consecutive = number.ToString().PadLeft(consecutiveLength, '0');
            var code = $"{prefix}-{year}-{consecutive}";

            if (!await existsAsync(code, cancellationToken))
                return code;
        }

        throw new InvalidOperationException(
            $"No hay códigos disponibles apra el prefijo '{prefix}'."
        );
    }

    private static string BuildPrefix(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = RemoveDiacritics(value.Trim()).ToUpperInvariant();

        var letters = normalized.Where(char.IsLetter).Take(3).ToArray();

        if (letters.Length == 0)
        {
            return fallback;
        }

        return new string(letters).PadRight(3, 'X');
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
