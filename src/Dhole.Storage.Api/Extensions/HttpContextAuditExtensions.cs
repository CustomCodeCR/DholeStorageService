namespace Dhole.Storage.Api.Extensions;

public static class HttpContextAuditExtensions
{
    public static Guid? GetCurrentUserId(this HttpContext httpContext)
    {
        var value = httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("user_id")?.Value
            ?? httpContext.User.FindFirst("nameidentifier")?.Value
            ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
