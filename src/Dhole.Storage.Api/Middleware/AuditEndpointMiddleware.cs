using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Storage.Persistence.Auditing;
using Dhole.Storage.Persistence.DbContexts;

namespace Dhole.Storage.Api.Middleware;

public sealed class AuditEndpointMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        // This middleware only audits read/review endpoints.
        // Write operations already create domain audit events through SaveChanges/outbox.
        // Auditing POST/PUT/PATCH/DELETE here would create duplicate AuditLog rows.
        if (!context.Request.Path.StartsWithSegments("/api") || !HttpMethods.IsGet(context.Request.Method))
        {
            return;
        }

        var dbContext = context.RequestServices.GetService<ServiceDbContext>();
        if (dbContext is null)
        {
            return;
        }

        var auditContext = AuditExecutionContextAccessor.Current;
        var entityId = ResolveEntityId(context);
        var entityType = ResolveEntityType(context);
        var action = ResolveAction(context.Request.Method);
        var eventId = Guid.NewGuid();

        var payload = new
        {
            EventId = eventId,
            CorrelationId = auditContext?.CorrelationId ?? Guid.NewGuid(),
            SourceService = "DholeStorageService",
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            EventType = $"http.{context.Request.Method.ToLowerInvariant()}",
            UserId = auditContext?.UserId,
            UserName = auditContext?.UserName,
            IpAddress = auditContext?.IpAddress,
            UserAgent = auditContext?.UserAgent,
            OccurredAt = DateTime.UtcNow,
            BeforeJson = (string?)null,
            AfterJson = (string?)null,
            PayloadJson = JsonSerializer.Serialize(new
            {
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value,
                StatusCode = context.Response.StatusCode,
                Endpoint = context.GetEndpoint()?.DisplayName
            }),
            Metadata = JsonSerializer.Serialize(new
            {
                RouteValues = context.Request.RouteValues.ToDictionary(x => x.Key, x => x.Value?.ToString()),
                Query = context.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString())
            }),
            ErrorMessage = context.Response.StatusCode >= 400 ? $"HTTP {context.Response.StatusCode}" : null,
            StackTrace = (string?)null,
            Details = Array.Empty<object>(),
        };

        dbContext.OutboxMessages.Add(
            new OutboxMessage
            {
                EventId = Guid.NewGuid(),
                EventType = "Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest",
                EventName = "audit.event.registered",
                SourceService = "DholeStorageService",
                PayloadJson = JsonSerializer.Serialize(payload),
                HeadersJson = null,
                CorrelationId = null,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                ErrorMessage = null,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await dbContext.SaveChangesAsync(context.RequestAborted);
    }

    private static string ResolveAction(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => "viewed",
            "POST" => "created_or_executed",
            "PUT" => "updated",
            "PATCH" => "updated",
            "DELETE" => "deleted",
            _ => "executed",
        };
    }

    private static string? ResolveEntityType(HttpContext context)
    {
        var path = context.Request.Path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (path.Length >= 3)
        {
            return path[2];
        }

        return path.LastOrDefault();
    }

    private static Guid? ResolveEntityId(HttpContext context)
    {
        foreach (var key in new[] { "entityId", "globalEntityId", "userId", "roleId", "scopeId", "sessionId", "fileId", "providerId", "catalogGroupId", "catalogItemId", "departmentId", "departmentTypeId", "menuId", "menuItemId", "routeId", "parameterId", "featureFlagId" })
        {
            if (context.Request.RouteValues.TryGetValue(key, out var routeValue)
                && Guid.TryParse(routeValue?.ToString(), out var routeGuid))
            {
                return routeGuid;
            }

            if (context.Request.Query.TryGetValue(key, out var queryValue)
                && Guid.TryParse(queryValue.ToString(), out var queryGuid))
            {
                return queryGuid;
            }
        }

        return null;
    }
}
