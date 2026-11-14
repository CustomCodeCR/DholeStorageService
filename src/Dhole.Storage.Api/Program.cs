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
app.UseMiddleware<AuditExecutionContextMiddleware>();
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
app.MapStorageEnvironmentEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await StorageDatabaseInitializer.InitializeAsync(dbContext, builder.Configuration);
}

// Dhole API Manager: authenticated runtime inventory, not a public proxy.
app.MapGet("/api/internal/third-party-route-catalog", (HttpContext http) =>
{
    var allowed = http.User.Identity?.IsAuthenticated == true &&
        http.User.Claims.Where(c => c.Type is "scope" or "scp" or "scopes")
            .SelectMany(c => c.Value.Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries))
            .Contains("integrations.thirdparty.manage", StringComparer.OrdinalIgnoreCase);
    if (!allowed) return (IResult)Results.StatusCode(StatusCodes.Status403Forbidden);
    var routes = new List<object>();
    foreach (var source in ((Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)app).DataSources)
        foreach (var endpoint in source.Endpoints.OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>())
        {
            var methods = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods;
            if (methods is null || endpoint.RoutePattern.RawText is not { } path) continue;
            foreach (var method in methods)
                if (method is "GET" or "POST" or "PUT" or "PATCH" or "DELETE")
                    routes.Add(new { method, path = "/" + path.TrimStart('/') });
        }
    return (IResult)Results.Ok(routes);
}).ExcludeFromDescription();

await app.RunAsync();

static int ReadPositiveInt(string? value, int fallback)
{
    return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}

static long ReadPositiveLong(string? value, long fallback)
{
    return long.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}
