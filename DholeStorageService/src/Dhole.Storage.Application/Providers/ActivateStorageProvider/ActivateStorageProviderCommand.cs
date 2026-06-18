using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Storage.Application.Providers.ActivateStorageProvider;

public sealed record ActivateStorageProviderCommand(Guid Id, Guid? ActivatedBy) : ICommand<Result>;
