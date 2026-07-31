using System.Security.Cryptography;
using Dhole.Storage.Application.Abstractions.Services;

namespace Dhole.Storage.Application.Services;

public sealed class ChecksumService : IChecksumService
{
    public async Task<string> CalculateAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanSeek)
            stream.Position = 0;

        var hash = await SHA256.HashDataAsync(stream, cancellationToken);

        if (stream.CanSeek)
            stream.Position = 0;

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
