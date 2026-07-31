namespace Dhole.Storage.Application.Abstractions.Auditing;

public interface IStorageAuditService
{
    Task PublishAsync(StorageAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
