using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Application.Providers.CreateStorageProvider;

public sealed record CreateStorageProviderCommand(
    CreateStorageProviderRequest Request,
    Guid? CreatedBy
) : ICommand<Result<Guid>>;
