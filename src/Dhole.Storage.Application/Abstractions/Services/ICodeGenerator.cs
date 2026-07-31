namespace Dhole.Storage.Application.Abstractions.Services;

public interface ICodeGenerator
{
    Task<string> GenerateProviderCodeAsync(
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    );
}
