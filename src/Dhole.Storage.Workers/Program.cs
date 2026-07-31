using CustomCodeFramework.Core.Abstractions;
using Dhole.Storage.Infrastructure.Time;
using Dhole.Storage.Persistence.DbContexts;
using Dhole.Storage.Persistence.DependencyInjection;
using Dhole.Storage.Persistence.Initialization;
using Dhole.Storage.Workers.DependencyInjection;
using Dhole.Storage.Workers.Security;

var contentRoot = Path.Combine(Directory.GetCurrentDirectory(), "src", "Dhole.Storage.Workers");
if (!Directory.Exists(contentRoot))
{
    contentRoot = Directory.GetCurrentDirectory();
}

var builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings { Args = args, ContentRootPath = contentRoot }
);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(contentRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true
    )
    .AddEnvironmentVariables();

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<ICurrentUser, WorkerCurrentUser>();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddStorageWorkers(builder.Configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await StorageDatabaseInitializer.InitializeAsync(dbContext, builder.Configuration);
}

await host.RunAsync();
