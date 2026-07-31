using System.Text.Json;
using Dhole.Storage.Domain.Providers.Entities;
using Dhole.Storage.Domain.Providers.Enums;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dhole.Storage.Persistence.Initialization;

public static class StorageDatabaseInitializer
{
    public static async Task InitializeAsync(
        ServiceDbContext dbContext,
        IConfiguration configuration,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Providers.AnyAsync(x => !x.IsDeleted, cancellationToken))
        {
            return;
        }

        var configuredRootPath = configuration["Storage:Local:RootPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage", "objects");
        var rootPath = Path.GetFullPath(configuredRootPath);
        Directory.CreateDirectory(rootPath);
        var providerConfiguration = JsonSerializer.Serialize(new { rootPath });
        var provider = Provider.Create(
            "STO-LOCAL-DEFAULT",
            "Almacenamiento local predeterminado",
            ProviderType.Local,
            providerConfiguration,
            isDefault: true,
            createdBy: null
        );

        dbContext.Providers.Add(provider);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // API y Worker pueden iniciar al mismo tiempo. Si el otro proceso ya
            // creó el proveedor predeterminado, la inicialización se considera completa.
            dbContext.ChangeTracker.Clear();
            if (!await dbContext.Providers.AsNoTracking().AnyAsync(
                    x => !x.IsDeleted,
                    cancellationToken
                ))
            {
                throw;
            }
        }
    }
}
