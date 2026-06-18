using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Application.Providers.UpdateStorageProvider;

public sealed record UpdateStorageProviderCommand(
    Guid Id,
    UpdateStorageProviderRequest Request,
    Guid? UpdatedBy
) : ICommand<Result>;
