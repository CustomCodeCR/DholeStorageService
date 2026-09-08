using Dhole.Storage.Persistence.DbContexts;
using Dhole.Storage.Persistence.Initialization;

namespace Dhole.Storage.Api.Endpoints;

public static class StorageEnvironmentEndpoints
{
    private const string ServiceKeyHeader = "X-Internal-Service-Key";

    public static IEndpointRouteBuilder MapStorageEnvironmentEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        app.MapPost(
                "/api/internal/storage/environment-reseed",
                async (
                    HttpContext httpContext,
                    ServiceDbContext dbContext,
                    IConfiguration configuration,
                    CancellationToken cancellationToken
                ) =>
                {
                    var expectedKey = configuration["INTERNAL_SERVICE_KEY"]
                        ?? configuration["InternalServices:ServiceKey"];
                    var suppliedKey = httpContext.Request.Headers[ServiceKeyHeader].ToString();

                    if (
                        string.IsNullOrWhiteSpace(expectedKey)
                        || !string.Equals(suppliedKey, expectedKey, StringComparison.Ordinal)
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var result = await StorageDatabaseInitializer.InitializeAsync(
                        dbContext,
                        configuration,
                        cancellationToken
                    );

                    return Results.Ok(
                        new
                        {
                            providerRestored = result.ProviderSynchronized,
                            providerCode = result.ProviderCode,
                            providerType = result.ProviderType,
                            isDefault = result.IsDefault,
                            secretValuesReturned = false,
                        }
                    );
                }
            )
            .WithTags("Internal")
            .AllowAnonymous();

        return app;
    }
}
