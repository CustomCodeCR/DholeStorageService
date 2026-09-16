using System.Security.Claims;
using Dhole.Storage.Api.Authorization;
using Dhole.Storage.Api.Services;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Api.Endpoints;

public static class MarketingStorageEndpoints
{
    public static IEndpointRouteBuilder MapMarketingStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/storage/marketing")
            .WithTags("Marketing Storage");

        group.MapPost(
            "/files",
            async (
                HttpRequest request,
                HttpContext httpContext,
                MarketingStorageApplicationService service,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    if (!request.HasFormContentType)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Storage.InvalidContentType",
                            detail: "La carga debe enviarse como multipart/form-data.");
                    }

                    var form = await request.ReadFormAsync(cancellationToken);
                    var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
                    if (file is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Storage.MissingFile",
                            detail: "Debe adjuntar un archivo en el campo 'file'.");
                    }

                    var sourceService = form["sourceService"].ToString();
                    var entityType = form["entityType"].ToString();
                    if (!Guid.TryParse(form["entityId"].ToString(), out var entityId))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Storage.InvalidEntityId",
                            detail: "entityId debe ser un GUID válido.");
                    }

                    Guid? providerId = null;
                    var rawProviderId = form["providerId"].ToString();
                    if (!string.IsNullOrWhiteSpace(rawProviderId))
                    {
                        if (!Guid.TryParse(rawProviderId, out var parsedProviderId))
                        {
                            return Results.Problem(
                                statusCode: StatusCodes.Status400BadRequest,
                                title: "Storage.InvalidProviderId",
                                detail: "providerId debe ser un GUID válido.");
                        }
                        providerId = parsedProviderId;
                    }

                    var result = await service.UploadAsync(
                        file,
                        sourceService,
                        entityType,
                        entityId,
                        providerId,
                        NullIfWhiteSpace(form["metadataJson"].ToString()),
                        GetCurrentUserId(httpContext),
                        cancellationToken);

                    return Results.Created($"/api/v1/storage/files/{result.File.Id}", result);
                }
                catch (InvalidOperationException exception)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Storage.InvalidMarketingMedia",
                        detail: exception.Message);
                }
                catch (Exception exception)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Storage.MarketingMediaFailed",
                        detail: exception.Message);
                }
            })
            .DisableAntiforgery()
            .RequireScope(StorageConstants.Scopes.FilesCreate);

        return app;
    }

    private static Guid? GetCurrentUserId(HttpContext context)
    {
        var raw = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue("userId");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private static string? NullIfWhiteSpace(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
