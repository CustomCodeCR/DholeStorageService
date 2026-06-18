using System.Security.Cryptography;
using Dhole.Storage.Application.Abstractions.Checksums;

namespace Dhole.Storage.Application.Checksums;

public sealed class FileChecksumService : IFileChecksumService
{
    public async Task<string> CalculateAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var hash = await SHA256.HashDataAsync(stream, cancellationToken);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
