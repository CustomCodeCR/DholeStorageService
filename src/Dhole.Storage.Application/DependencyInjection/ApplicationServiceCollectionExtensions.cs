using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;
using Dhole.Storage.Application.Abstractions.Checksums;
using Dhole.Storage.Application.Abstractions.Locks;
using Dhole.Storage.Application.Abstractions.Naming;
using Dhole.Storage.Application.Abstractions.Paths;
using Dhole.Storage.Application.Checksums;
using Dhole.Storage.Application.Locks;
using Dhole.Storage.Application.Naming;
using Dhole.Storage.Application.Paths;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Storage.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCustomCodeValidation(AssemblyReference.Assembly);

        services.AddCustomCodeCqrs(AssemblyReference.Assembly);
        services.AddCustomCodeCqrsBehaviors();

        services.AddSingleton<IStorageFileNameGenerator, StorageFileNameGenerator>();
        services.AddSingleton<IStoragePathResolver, StoragePathResolver>();
        services.AddSingleton<IFileChecksumService, FileChecksumService>();
        services.AddSingleton<IStorageLockService, InMemoryStorageLockService>();

        return services;
    }
}
