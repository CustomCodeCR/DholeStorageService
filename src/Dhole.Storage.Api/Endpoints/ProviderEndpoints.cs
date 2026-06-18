using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Storage.Application.Providers.ActivateStorageProvider;
using Dhole.Storage.Application.Providers.CreateStorageProvider;
using Dhole.Storage.Application.Providers.DeleteStorageProvider;
using Dhole.Storage.Application.Providers.GetStorageProviderById;
using Dhole.Storage.Application.Providers.GetStorageProviders;
using Dhole.Storage.Application.Providers.GetStorageProvidersForSelect;
using Dhole.Storage.Application.Providers.InactivateStorageProvider;
using Dhole.Storage.Application.Providers.UpdateStorageProvider;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Api.Endpoints;

public static class ProviderEndpoints
{
    public static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storage/providers")
            .WithTags("Storage Providers")
            .RequireAuthorization();

        group.MapPost(
            "/",
            async (
                CreateStorageProviderRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new CreateStorageProviderCommand(request, CreatedBy: null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetStorageProviderByIdQuery(id),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapGet(
            "/",
            async (
                int pageNumber,
                int pageSize,
                string? search,
                string? providerType,
                bool? isActive,
                IQueryDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetStorageProvidersQuery(
                        PageRequest.Create(pageNumber, pageSize),
                        search,
                        providerType,
                        isActive
                    ),
                    cancellationToken
                );

                return EndpointResults.FromPaged(result);
            }
        );

        group.MapGet(
            "/select",
            async (
                string? providerType,
                string? search,
                IQueryDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetStorageProvidersForSelectQuery(providerType, search),
                    cancellationToken
                );

                return EndpointResults.Ok(result);
            }
        );

        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateStorageProviderRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new UpdateStorageProviderCommand(id, request, UpdatedBy: null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapPost(
            "/{id:guid}/activate",
            async (
                Guid id,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new ActivateStorageProviderCommand(id, ActivatedBy: null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapPost(
            "/{id:guid}/inactivate",
            async (
                Guid id,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new InactivateStorageProviderCommand(id, InactivatedBy: null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapDelete(
            "/{id:guid}",
            async (
                Guid id,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new DeleteStorageProviderCommand(id, DeletedBy: null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        return app;
    }
}
