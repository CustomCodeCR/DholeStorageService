using System.Security.Claims;
using Dhole.Storage.Application.Abstractions.Auditing;
using Dhole.Storage.Persistence.DbContexts;

namespace Dhole.Storage.Api.Middleware;

public sealed class AuditEndpointMiddleware(
    RequestDelegate next,
    ILogger<AuditEndpointMiddleware> logger
)
{
    private static readonly string[] IgnoredPathPrefixes = ["/health", "/swagger", "/metrics", "/favicon.ico"];

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        if (!ShouldAudit(context)) return;

        try
        {
            var auditService = context.RequestServices.GetRequiredService<IStorageAuditService>();
            var dbContext = context.RequestServices.GetRequiredService<ServiceDbContext>();
            var entityId = ResolveEntityId(context);
            var entityType = ResolveEntityType(context);
            var action = ResolveAction(context);

            await auditService.PublishAsync(
                new StorageAuditEvent(
                    EventType: $"storage.http.{entityType.ToLowerInvariant()}.{action}",
                    Action: action,
                    EntityType: entityType,
                    EntityId: entityId,
                    ActorUserId: ResolveUserId(context.User),
                    ActorUserName: ResolveUserName(context.User),
                    Payload: new
                    {
                        Method = context.Request.Method,
                        Path = context.Request.Path.Value,
                        QueryString = context.Request.QueryString.Value,
                        StatusCode = context.Response.StatusCode,
                        Endpoint = context.GetEndpoint()?.DisplayName,
                    },
                    Metadata: new
                    {
                        AuditLayer = "endpoint",
                        RouteValues = context.Request.RouteValues.ToDictionary(x => x.Key, x => x.Value?.ToString()),
                        Query = context.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
                        context.TraceIdentifier,
                    },
                    ErrorMessage: context.Response.StatusCode >= 400 ? $"HTTP {context.Response.StatusCode}" : null
                ),
                CancellationToken.None
            );

            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to audit Storage action {Method} {Path}.", context.Request.Method, context.Request.Path.Value);
        }
    }

    private static bool ShouldAudit(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api")) return false;
        var path = context.Request.Path.Value ?? string.Empty;
        return !IgnoredPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveAction(HttpContext context)
    {
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized) return "unauthorized";
        if (context.Response.StatusCode == StatusCodes.Status403Forbidden) return "forbidden";
        if (context.Response.StatusCode >= 500) return "http_error";

        return context.Request.Method.ToUpperInvariant() switch
        {
            "GET" or "HEAD" => "viewed",
            "POST" => "created",
            "PUT" or "PATCH" => "updated",
            "DELETE" => "deleted",
            _ => "executed",
        };
    }

    private static string ResolveEntityType(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (path.Contains("/providers")) return "Provider";
        if (path.Contains("/files")) return "File";
        return "Storage";
    }

    private static Guid? ResolveEntityId(HttpContext context)
    {
        foreach (var key in new[] { "id", "fileId", "providerId", "entityId" })
        {
            if (context.Request.RouteValues.TryGetValue(key, out var routeValue) && Guid.TryParse(routeValue?.ToString(), out var routeGuid))
                return routeGuid;
            if (context.Request.Query.TryGetValue(key, out var queryValue) && Guid.TryParse(queryValue.ToString(), out var queryGuid))
                return queryGuid;
        }

        foreach (var segment in context.Request.Path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [])
            if (Guid.TryParse(segment, out var guid)) return guid;

        return null;
    }

    private static Guid? ResolveUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private static string? ResolveUserName(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue("name") ?? user.Identity?.Name;
}
