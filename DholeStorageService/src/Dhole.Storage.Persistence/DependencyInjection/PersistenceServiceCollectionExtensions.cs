using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using Dhole.Storage.Application.Abstractions.Messaging;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Persistence.DbContexts;
using Dhole.Storage.Persistence.Messaging;
using Dhole.Storage.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Storage.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodePostgres(configuration);
        services.AddCustomCodePostgresEntityFramework<ServiceDbContext>();

        services.AddScoped<IStorageFileRepository, StorageFileRepository>();
        services.AddScoped<IStorageProviderRepository, StorageProviderRepository>();

        services.AddScoped<IIntegrationEventOutboxWriter, IntegrationEventOutboxWriter>();

        return services;
    }
}
