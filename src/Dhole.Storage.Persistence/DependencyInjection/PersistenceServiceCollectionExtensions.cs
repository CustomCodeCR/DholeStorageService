using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using Dhole.Storage.Application.Abstractions.Auditing;
using Dhole.Storage.Application.Abstractions.Messaging;
using Dhole.Storage.Application.Abstractions.Repositories;
using Dhole.Storage.Persistence.Auditing;
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

        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();

        services.AddScoped<IIntegrationEventOutboxWriter, IntegrationEventOutboxWriter>();
        services.AddScoped<IStorageAuditService, StorageAuditService>();

        return services;
    }
}
