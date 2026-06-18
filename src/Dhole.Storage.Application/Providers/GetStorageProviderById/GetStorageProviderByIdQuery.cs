using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Storage.Contracts.Providers;

namespace Dhole.Storage.Application.Providers.GetStorageProviderById;

public sealed record GetStorageProviderByIdQuery(Guid Id) : IQuery<Result<StorageProviderDto>>;
