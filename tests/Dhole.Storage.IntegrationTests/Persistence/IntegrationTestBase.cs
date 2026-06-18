using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Storage.IntegrationTests;

[TestClass]
[DoNotParallelize]
public abstract class IntegrationTestBase
{
    protected ServiceDbContext DbContext { get; private set; } = default!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DHOLE_STORAGE_TEST_POSTGRES")
            ?? "Host=localhost;Port=5432;Database=dhole_storage_tests;Username=postgres;Password=turbodiesel";

        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        DbContext = new ServiceDbContext(options);

        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
    }
}
