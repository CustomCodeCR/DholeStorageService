using CustomCodeFramework.Core.Abstractions;
using Dhole.Storage.Api.Endpoints;
using Dhole.Storage.Api.Middleware;
using Dhole.Storage.Api.Services;
using Dhole.Storage.Application.DependencyInjection;
using Dhole.Storage.Infrastructure.DependencyInjection;
using Dhole.Storage.Infrastructure.Time;
using Dhole.Storage.Persistence.DbContexts;
using Dhole.Storage.Persistence.DependencyInjection;
using Dhole.Storage.Persistence.Initialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "storage-cors";
var httpPort = ReadPositiveInt(builder.Configuration["Http:Port"], 5207);
var maximumFileSizeBytes = ReadPositiveLong(
    builder.Configuration["Storage:MaximumFileSizeBytes"],
    100L * 1024L * 1024L
);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maximumFileSizeBytes + 1024L * 1024L;
    options.ListenAnyIP(httpPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        CorsPolicyName,
        policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (origins is { Length: > 0 })
            {
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            }
            else
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }
        }
    );
});

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<StorageFileApplicationService>();
builder.Services.AddScoped<StorageProviderApplicationService>();

var app = builder.Build();

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditEndpointMiddleware>();

app.MapGet(
    "/health",
    () => Results.Ok(new
    {
        status = "Healthy",
        service = "DholeStorageService",
        port = httpPort,
        maximumFileSizeBytes,
    })
).AllowAnonymous();

app.MapGet(
    "/api/v1/ping",
    () => Results.Ok(new { service = "Dhole.Storage", status = "ok", port = httpPort })
).AllowAnonymous();

app.MapStorageEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await StorageDatabaseInitializer.InitializeAsync(dbContext, builder.Configuration);
}

await app.RunAsync();

static int ReadPositiveInt(string? value, int fallback)
{
    return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}

static long ReadPositiveLong(string? value, long fallback)
{
    return long.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}
