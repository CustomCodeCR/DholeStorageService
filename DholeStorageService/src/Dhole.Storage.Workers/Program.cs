using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Storage.AzureBlob.DependencyInjection;
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.Local.DependencyInjection;
using CustomCodeFramework.Storage.S3.DependencyInjection;
using Dhole.Storage.Application.DependencyInjection;
using Dhole.Storage.Infrastructure.Time;
using Dhole.Storage.Persistence.DependencyInjection;
using Dhole.Storage.Worker.DependencyInjection;
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

builder
    .Configuration.SetBasePath(contentRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine($"Postgres: {builder.Configuration["Postgres:ConnectionString"]}");

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<ICurrentUser, WorkerCurrentUser>();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddCustomCodeStorage(builder.Configuration);
builder.Services.AddCustomCodeLocalStorage(builder.Configuration);
builder.Services.AddCustomCodeAzureBlobStorage(builder.Configuration);
builder.Services.AddCustomCodeS3Storage(builder.Configuration);

builder.Services.AddStorageWorker(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
