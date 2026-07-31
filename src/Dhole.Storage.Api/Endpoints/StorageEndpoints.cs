using System.Security.Claims;
using Dhole.Storage.Api.Authorization;
using Dhole.Storage.Api.Services;
using Dhole.Storage.Contracts.Providers.Request;
using Dhole.Storage.Domain.Shared;

namespace Dhole.Storage.Api.Endpoints;

public static class StorageEndpoints
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/storage").WithTags("Storage");

        group.MapPost(
            "/files",
            async (
                HttpRequest request,
                HttpContext httpContext,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    if (
                        httpContext.User.Identity?.IsAuthenticated == true
                        && !HasScope(httpContext, StorageConstants.Scopes.FilesCreate)
                    )
                    {
                        return Problem(
                            httpContext,
                            StatusCodes.Status403Forbidden,
                            "Storage.Forbidden",
                            $"Se requiere el scope {StorageConstants.Scopes.FilesCreate}."
                        );
                    }

                    if (!request.HasFormContentType)
                    {
                        return Problem(
                            httpContext,
                            StatusCodes.Status400BadRequest,
                            "Storage.InvalidContentType",
                            "La carga debe enviarse como multipart/form-data."
                        );
                    }

                    var form = await request.ReadFormAsync(cancellationToken);
                    var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
                    if (file is null)
                    {
                        return Problem(
                            httpContext,
                            StatusCodes.Status400BadRequest,
                            "Storage.MissingFile",
                            "Debe adjuntar un archivo en el campo 'file'."
                        );
                    }

                    var sourceService = form["sourceService"].ToString();
                    var entityType = form["entityType"].ToString();
                    if (!Guid.TryParse(form["entityId"].ToString(), out var entityId))
                    {
                        return Problem(
                            httpContext,
                            StatusCodes.Status400BadRequest,
                            "Storage.InvalidEntityId",
                            "entityId debe ser un GUID válido."
                        );
                    }

                    Guid? providerId = null;
                    var providerValue = form["providerId"].ToString();
                    if (!string.IsNullOrWhiteSpace(providerValue))
                    {
                        if (!Guid.TryParse(providerValue, out var parsedProviderId))
                        {
                            return Problem(
                                httpContext,
                                StatusCodes.Status400BadRequest,
                                "Storage.InvalidProviderId",
                                "providerId debe ser un GUID válido."
                            );
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
                        cancellationToken
                    );

                    return Results.Created($"/api/v1/storage/files/{result.Id}", result);
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).DisableAntiforgery().AllowAnonymous();

        group.MapPost(
            "/files/{fileId:guid}/versions",
            async (
                Guid fileId,
                HttpRequest request,
                HttpContext httpContext,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    if (!request.HasFormContentType)
                    {
                        return Problem(
                            httpContext,
                            StatusCodes.Status400BadRequest,
                            "Storage.InvalidContentType",
                            "La carga debe enviarse como multipart/form-data."
                        );
                    }

                    var form = await request.ReadFormAsync(cancellationToken);
                    var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
                    if (file is null)
                    {
                        return Problem(
                            httpContext,
                            StatusCodes.Status400BadRequest,
                            "Storage.MissingFile",
                            "Debe adjuntar un archivo en el campo 'file'."
                        );
                    }

                    var result = await service.UploadVersionAsync(
                        fileId,
                        file,
                        GetCurrentUserId(httpContext),
                        cancellationToken
                    );
                    return Results.Ok(result);
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).DisableAntiforgery().RequireScope(StorageConstants.Scopes.FilesVersion);

        group.MapGet(
            "/files",
            async (
                int? pageNumber,
                int? pageSize,
                string? search,
                string? contentType,
                string? sourceService,
                string? entityType,
                Guid? providerId,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) => Results.Ok(
                await service.BrowseAsync(
                    pageNumber ?? 1,
                    pageSize ?? 25,
                    search,
                    contentType,
                    sourceService,
                    entityType,
                    providerId,
                    cancellationToken
                )
            )
        ).RequireScope(StorageConstants.Scopes.FilesView);

        group.MapGet(
            "/files/summary",
            async (
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) => Results.Ok(await service.GetSummaryAsync(cancellationToken))
        ).RequireScope(StorageConstants.Scopes.FilesView);

        group.MapGet(
            "/files/{fileId:guid}",
            async (
                Guid fileId,
                HttpContext httpContext,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await service.GetAsync(fileId, cancellationToken);
                return result is null
                    ? Problem(
                        httpContext,
                        StatusCodes.Status404NotFound,
                        "Storage.FileNotFound",
                        "El archivo no existe."
                    )
                    : Results.Ok(result);
            }
        ).RequireScope(StorageConstants.Scopes.FilesView);

        group.MapGet(
            "/files/{fileId:guid}/content",
            async (
                Guid fileId,
                HttpContext httpContext,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    if (
                        httpContext.User.Identity?.IsAuthenticated == true
                        && !HasScope(httpContext, StorageConstants.Scopes.FilesDownload)
                    )
                    {
                        return Problem(
                            httpContext,
                            StatusCodes.Status403Forbidden,
                            "Storage.Forbidden",
                            $"Se requiere el scope {StorageConstants.Scopes.FilesDownload}."
                        );
                    }

                    var result = await service.DownloadAsync(fileId, cancellationToken);
                    return Results.File(
                        result.Content,
                        result.ContentType,
                        result.FileName,
                        enableRangeProcessing: true
                    );
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).AllowAnonymous();

        group.MapGet(
            "/files/by-entity",
            async (
                string sourceService,
                string entityType,
                Guid entityId,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) => Results.Ok(
                await service.GetByEntityAsync(
                    sourceService,
                    entityType,
                    entityId,
                    cancellationToken
                )
            )
        ).RequireScope(StorageConstants.Scopes.FilesView);

        group.MapPut(
            "/files/{fileId:guid}/current-version/{versionNumber:int}",
            async (
                Guid fileId,
                int versionNumber,
                HttpContext httpContext,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    await service.ChangeCurrentVersionAsync(
                        fileId,
                        versionNumber,
                        GetCurrentUserId(httpContext),
                        cancellationToken
                    );
                    return Results.NoContent();
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).RequireScope(StorageConstants.Scopes.FilesVersion);

        group.MapDelete(
            "/files/{fileId:guid}",
            async (
                Guid fileId,
                HttpContext httpContext,
                StorageFileApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    await service.DeleteAsync(
                        fileId,
                        GetCurrentUserId(httpContext),
                        cancellationToken
                    );
                    return Results.NoContent();
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).RequireScope(StorageConstants.Scopes.FilesDelete);

        var providers = group.MapGroup("/providers").WithTags("Storage Providers");

        providers.MapGet(
            "",
            async (
                StorageProviderApplicationService service,
                CancellationToken cancellationToken
            ) => Results.Ok(await service.GetAllAsync(cancellationToken))
        ).RequireScope(StorageConstants.Scopes.ProvidersView);

        providers.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                HttpContext httpContext,
                StorageProviderApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await service.GetAsync(id, cancellationToken);
                return result is null
                    ? Problem(
                        httpContext,
                        StatusCodes.Status404NotFound,
                        "Storage.ProviderNotFound",
                        "El proveedor no existe."
                    )
                    : Results.Ok(result);
            }
        ).RequireScope(StorageConstants.Scopes.ProvidersView);

        providers.MapPost(
            "",
            async (
                CreateProviderRequest request,
                HttpContext httpContext,
                StorageProviderApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    var result = await service.CreateAsync(
                        request,
                        GetCurrentUserId(httpContext),
                        cancellationToken
                    );
                    return Results.Created($"/api/v1/storage/providers/{result.Id}", result);
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).RequireScope(StorageConstants.Scopes.ProvidersCreate);

        providers.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateProviderRequest request,
                HttpContext httpContext,
                StorageProviderApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    return Results.Ok(
                        await service.UpdateAsync(
                            id,
                            request,
                            GetCurrentUserId(httpContext),
                            cancellationToken
                        )
                    );
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).RequireScope(StorageConstants.Scopes.ProvidersUpdate);

        providers.MapPut(
            "/{id:guid}/active/{isActive:bool}",
            async (
                Guid id,
                bool isActive,
                HttpContext httpContext,
                StorageProviderApplicationService service,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    return Results.Ok(
                        await service.SetActiveAsync(
                            id,
                            isActive,
                            GetCurrentUserId(httpContext),
                            cancellationToken
                        )
                    );
                }
                catch (Exception exception)
                {
                    return FromException(httpContext, exception);
                }
            }
        ).RequireScope(StorageConstants.Scopes.ProvidersSetActive);

        return app;
    }

    private static IResult FromException(HttpContext context, Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => Problem(
                context,
                StatusCodes.Status404NotFound,
                "Storage.NotFound",
                exception.Message
            ),
            FileNotFoundException => Problem(
                context,
                StatusCodes.Status404NotFound,
                "Storage.ContentNotFound",
                exception.Message
            ),
            InvalidOperationException => Problem(
                context,
                StatusCodes.Status400BadRequest,
                "Storage.InvalidOperation",
                exception.Message
            ),
            _ => Problem(
                context,
                StatusCodes.Status500InternalServerError,
                "Storage.UnhandledError",
                exception.Message
            ),
        };
    }

    private static IResult Problem(
        HttpContext context,
        int status,
        string code,
        string detail
    )
    {
        return Results.Problem(
            statusCode: status,
            title: code,
            detail: detail,
            instance: context.Request.Path,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = code,
                ["traceId"] = context.TraceIdentifier,
            }
        );
    }

    private static Guid? GetCurrentUserId(HttpContext context)
    {
        var value = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static bool HasScope(HttpContext context, string requiredScope)
    {
        return context.User.Claims.Any(claim =>
            IsScopeClaim(claim.Type)
            && claim
                .Value.Split(
                    new[] { ' ', ',', ';' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
                .Contains(requiredScope, StringComparer.OrdinalIgnoreCase)
        );
    }

    private static bool IsScopeClaim(string claimType)
    {
        return claimType.Equals("scope", StringComparison.OrdinalIgnoreCase)
            || claimType.Equals("scp", StringComparison.OrdinalIgnoreCase)
            || claimType.Equals("scopes", StringComparison.OrdinalIgnoreCase)
            || claimType.EndsWith("/scope", StringComparison.OrdinalIgnoreCase)
            || claimType.EndsWith("/scopes", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
