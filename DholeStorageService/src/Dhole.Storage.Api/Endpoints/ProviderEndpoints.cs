using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Storage.Application.Providers.ActivateStorageProvider;
using Dhole.Storage.Application.Providers.CreateStorageProvider;
using Dhole.Storage.Application.Providers.DeleteStorageProvider;
using Dhole.Storage.Application.Providers.GetStorageProviderById;
using Dhole.Storage.Application.Providers.GetStorageProviders;
using Dhole.Storage.Application.Providers.GetStorageProvidersForSelect;
using Dhole.Storage.Application.Providers.InactivateStorageProvider;
using Dhole.Storage.Application.Providers.UpdateStorageProvider;
using Dhole.Storage.Contracts.Providers;

using Dhole.Storage.Api.Audit;

using Dhole.Storage.Api.Security;

namespace Dhole.Storage.Api.Endpoints;

public static class ProviderEndpoints
{
    public static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storage/providers")
            .WithTags("Storage Providers").AddEndpointFilter(new AuditEndpointFilter("DholeStorageService"))
            .RequireAuthorization();

        group.MapPost(
            "/",
            async (
                CreateStorageProviderRequest request,
                ICurrentUser currentUser,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new CreateStorageProviderCommand(request, CreatedBy: currentUser.GetUserGuid()),
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
                ICurrentUser currentUser,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new UpdateStorageProviderCommand(id, request, UpdatedBy: currentUser.GetUserGuid()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapPost(
            "/{id:guid}/activate",
            async (
                Guid id,
                ICurrentUser currentUser,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new ActivateStorageProviderCommand(id, ActivatedBy: currentUser.GetUserGuid()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapPost(
            "/{id:guid}/inactivate",
            async (
                Guid id,
                ICurrentUser currentUser,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new InactivateStorageProviderCommand(id, InactivatedBy: currentUser.GetUserGuid()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        group.MapDelete(
            "/{id:guid}",
            async (
                Guid id,
                ICurrentUser currentUser,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new DeleteStorageProviderCommand(id, DeletedBy: currentUser.GetUserGuid()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        );

        return app;
    }
}
