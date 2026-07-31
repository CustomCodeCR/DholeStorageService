using Dhole.Storage.Application.Abstractions.Services;
using Dhole.Storage.Application.Paths;
using Dhole.Storage.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Storage.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IChecksumService, ChecksumService>();
        services.AddSingleton<IFileNameGenerator, FileNameGenerator>();
        services.AddSingleton<IPathResolver, PathResolver>();
        services.AddSingleton<ICodeGenerator, CodeGenerator>();
        services.AddSingleton<ILockService, InMemoryLockService>();

        return services;
    }
}
