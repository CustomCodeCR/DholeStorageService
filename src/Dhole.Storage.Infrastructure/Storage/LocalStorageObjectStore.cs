using Dhole.Storage.Application.Abstractions.Storage;
using Dhole.Storage.Domain.Providers.Enums;
using Microsoft.Extensions.Configuration;

namespace Dhole.Storage.Infrastructure.Storage;

public sealed class LocalStorageObjectStore(IConfiguration configuration) : IStorageObjectStore
{
    public ProviderType ProviderType => ProviderType.Local;

    public async Task WriteAsync(
        string path,
        Stream content,
        string contentType,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var absolutePath = ResolveAbsolutePath(path, providerConfiguration);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await using var output = new FileStream(
            absolutePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan
        );

        await content.CopyToAsync(output, cancellationToken);
        await output.FlushAsync(cancellationToken);

        if (content.CanSeek)
        {
            content.Position = 0;
        }
    }

    public async Task<StorageObjectReadResult> ReadAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        var absolutePath = ResolveAbsolutePath(path, providerConfiguration);

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("El archivo físico no existe.", absolutePath);
        }

        return new StorageObjectReadResult(
            await File.ReadAllBytesAsync(absolutePath, cancellationToken)
        );
    }

    public Task DeleteAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolutePath = ResolveAbsolutePath(path, providerConfiguration);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string path,
        string? providerConfiguration,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ResolveAbsolutePath(path, providerConfiguration)));
    }

    private string ResolveAbsolutePath(string relativePath, string? providerConfiguration)
    {
        var providerOptions = StorageProviderConfiguration.Parse<LocalProviderOptions>(
            providerConfiguration
        );

        var configuredRoot = providerOptions.RootPath ?? configuration["Storage:Local:RootPath"];
        var rootPath = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(AppContext.BaseDirectory, "storage", "objects")
            : configuredRoot.Trim();

        rootPath = Path.GetFullPath(rootPath);
        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var absolutePath = Path.GetFullPath(Path.Combine(rootPath, normalizedRelativePath));
        var rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;

        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!absolutePath.StartsWith(rootPrefix, pathComparison))
        {
            throw new InvalidOperationException(
                "La ruta solicitada está fuera del directorio permitido de Storage."
            );
        }

        return absolutePath;
    }
}
