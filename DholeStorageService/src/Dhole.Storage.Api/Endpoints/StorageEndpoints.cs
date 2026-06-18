using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Storage.Application.Files.DeleteFile;
using Dhole.Storage.Application.Files.DownloadFile;
using Dhole.Storage.Application.Files.GetFileById;
using Dhole.Storage.Application.Files.GetFiles;
using Dhole.Storage.Application.Files.GetFilesForSelect;
using Dhole.Storage.Application.Files.UploadFile;
using Dhole.Storage.Contracts.Files;

using Dhole.Storage.Api.Security;

using Dhole.Storage.Api.Audit;

namespace Dhole.Storage.Api.Endpoints;

public static class StorageEndpoints
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storage").WithTags("Storage").AddEndpointFilter(new AuditEndpointFilter("DholeStorageService")).RequireAuthorization();

        group
            .MapPost(
                "/files",
                async (
                    HttpRequest request,
                    ICurrentUser currentUser,
                ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    if (!request.HasFormContentType)
                    {
                        return Results.BadRequest("The request must be multipart/form-data.");
                    }

                    var form = await request.ReadFormAsync(cancellationToken);
                    var file = form.Files.GetFile("file");

                    if (file is null || file.Length == 0)
                    {
                        return Results.BadRequest("File is required.");
                    }

                    if (!Guid.TryParse(form["providerId"].FirstOrDefault(), out var providerId))
                    {
                        return Results.BadRequest("providerId is required.");
                    }

                    if (!Guid.TryParse(form["entityId"].FirstOrDefault(), out var entityId))
                    {
                        return Results.BadRequest("entityId is required.");
                    }

                    var sourceService = form["sourceService"].FirstOrDefault();
                    var entityType = form["entityType"].FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(sourceService))
                    {
                        return Results.BadRequest("sourceService is required.");
                    }

                    if (string.IsNullOrWhiteSpace(entityType))
                    {
                        return Results.BadRequest("entityType is required.");
                    }

                    var referenceType = form["referenceType"].FirstOrDefault();
                    var metadataJson = form["metadataJson"].FirstOrDefault();

                    await using var stream = file.OpenReadStream();

                    var uploadRequest = new UploadFileRequest(
                        providerId,
                        sourceService,
                        entityType,
                        entityId,
                        referenceType,
                        metadataJson
                    );

                    var result = await dispatcher.DispatchAsync(
                        new UploadFileCommand(
                            uploadRequest,
                            file.FileName,
                            file.ContentType,
                            file.Length,
                            stream,
                            CreatedBy: currentUser.GetUserGuid()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .DisableAntiforgery();

        group.MapGet(
            "/files/{id:guid}",
            async (
                Guid id,
                IQueryDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetFileByIdQuery(id),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapGet(
            "/files",
            async (
                int pageNumber,
                int pageSize,
                string? sourceService,
                string? entityType,
                Guid? entityId,
                string? documentType,
                string? search,
                string? status,
                IQueryDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetFilesQuery(
                        PageRequest.Create(pageNumber, pageSize),
                        sourceService,
                        entityType,
                        entityId,
                        documentType,
                        search,
                        status
                    ),
                    cancellationToken
                );

                return EndpointResults.FromPaged(result);
            }
        );

        group.MapGet(
            "/files/select",
            async (
                string? sourceService,
                string? entityType,
                Guid? entityId,
                string? search,
                IQueryDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetFilesForSelectQuery(sourceService, entityType, entityId, search),
                    cancellationToken
                );

                return EndpointResults.Ok(result);
            }
        );

        group.MapGet(
            "/files/{id:guid}/download",
            async (
                Guid id,
                IQueryDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new DownloadFileQuery(id),
                    cancellationToken
                );

                if (result.IsFailure)
                {
                    return EndpointResults.FromResult(result, httpContext);
                }

                var file = result.Value;

                return Results.File(
                    file.Content,
                    file.ContentType,
                    file.FileName,
                    enableRangeProcessing: true
                );
            }
        );

        group.MapDelete(
            "/files/{id:guid}",
            async (
                Guid id,
                ICurrentUser currentUser,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new DeleteFileCommand(id, DeletedBy: currentUser.GetUserGuid()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        return app;
    }
}
