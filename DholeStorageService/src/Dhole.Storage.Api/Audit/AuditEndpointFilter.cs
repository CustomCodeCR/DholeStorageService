using System.Text.Json;
using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Storage.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;

namespace Dhole.Storage.Api.Audit;

internal sealed class AuditEndpointFilter(string sourceService) : IEndpointFilter
{
    private const string AuditEventType = "Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest";
    private const string AuditEventName = "audit.event.registered";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var result = await next(context);

        var statusCode = ExtractStatusCode(result);
        var success = statusCode is null or >= 200 and < 400;

        await WriteAuditAsync(httpContext, statusCode, success, null);

        return result;
    }

    private async Task WriteAuditAsync(HttpContext httpContext, int? statusCode, bool success, string? errorMessage)
    {
        var dbContext = httpContext.RequestServices.GetRequiredService<ServiceDbContext>();
        var currentUser = httpContext.RequestServices.GetService<ICurrentUser>();
        var routeValues = httpContext.Request.RouteValues;

        var entityId = ResolveEntityId(routeValues, httpContext.Request.Query);
        var userId = currentUser is null || !Guid.TryParse(currentUser.UserId, out var parsedUserId) ? (Guid?)null : parsedUserId;

        var auditPayload = new
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.TryParse(httpContext.TraceIdentifier, out var correlationId) ? correlationId : Guid.NewGuid(),
            SourceService = sourceService,
            EntityType = ResolveEntityType(httpContext),
            EntityId = entityId,
            Action = ResolveAction(httpContext.Request.Method),
            EventType = $"http.{httpContext.Request.Method.ToLowerInvariant()}",
            UserId = userId,
            UserName = currentUser?.UserName,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            OccurredAt = DateTime.UtcNow,
            BeforeJson = (string?)null,
            AfterJson = (string?)null,
            PayloadJson = JsonSerializer.Serialize(new
            {
                Method = httpContext.Request.Method,
                Path = httpContext.Request.Path.Value,
                QueryString = httpContext.Request.QueryString.Value,
                StatusCode = statusCode,
                Success = success
            }),
            Metadata = (string?)null,
            ErrorMessage = errorMessage,
            StackTrace = (string?)null,
            Details = Array.Empty<object>()
        };

        await dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            EventType = AuditEventType,
            EventName = AuditEventName,
            SourceService = sourceService,
            PayloadJson = JsonSerializer.Serialize(auditPayload),
            HeadersJson = null,
            CorrelationId = httpContext.TraceIdentifier,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow
        }, httpContext.RequestAborted);

        await dbContext.SaveChangesAsync(httpContext.RequestAborted);
    }

    private static int? ExtractStatusCode(object? result)
    {
        return result is IStatusCodeHttpResult statusCodeHttpResult ? statusCodeHttpResult.StatusCode : null;
    }

    private static Guid? ResolveEntityId(RouteValueDictionary routeValues, IQueryCollection query)
    {
        foreach (var key in routeValues.Keys.Where(x => x.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || string.Equals(x, "id", StringComparison.OrdinalIgnoreCase)))
        {
            if (Guid.TryParse(routeValues[key]?.ToString(), out var value))
            {
                return value;
            }
        }

        foreach (var key in query.Keys.Where(x => x.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || string.Equals(x, "id", StringComparison.OrdinalIgnoreCase)))
        {
            if (Guid.TryParse(query[key].FirstOrDefault(), out var value))
            {
                return value;
            }
        }

        return null;
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
            _ => method.ToLowerInvariant()
        };
    }

    private static string? ResolveEntityType(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return path is { Length: >= 3 } ? path[2] : path?.LastOrDefault();
    }
}
