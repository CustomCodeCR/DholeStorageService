using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Storage.Domain.Providers.Events;

namespace Dhole.Storage.Domain.Providers.Entities;

public sealed class StorageProvider : SoftDeletableAggregateRoot<Guid>
{
    private StorageProvider() { }

    private StorageProvider(
        Guid id,
        string code,
        string name,
        string providerType,
        string? configuration,
        bool isDefault,
        Guid? createdBy
    )
        : base(id)
    {
        Code = code;
        Name = name;
        ProviderType = providerType;
        Configuration = configuration;
        IsDefault = isDefault;
        IsActive = true;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string ProviderType { get; private set; } = default!;
    public string? Configuration { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public static StorageProvider Create(
        string code,
        string name,
        string providerType,
        string? configuration,
        bool isDefault,
        Guid? createdBy = null
    )
    {
        var entity = new StorageProvider(
            Guid.NewGuid(),
            code.Trim(),
            name.Trim(),
            providerType.Trim(),
            configuration,
            isDefault,
            createdBy
        );

        entity.AddDomainEvent(
            new StorageProviderCreatedDomainEvent(
                entity.Id,
                entity.Code,
                entity.Name,
                entity.ProviderType,
                createdBy
            )
        );

        return entity;
    }

    public void Update(string name, string? configuration, bool isDefault, Guid? updatedBy = null)
    {
        Name = name.Trim();
        Configuration = configuration;
        IsDefault = isDefault;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(
            new StorageProviderUpdatedDomainEvent(Id, Code, Name, ProviderType, updatedBy)
        );
    }

    public void Activate(Guid? activatedBy = null)
    {
        IsActive = true;

        MarkAsUpdated(DateTime.UtcNow, activatedBy?.ToString());

        AddDomainEvent(new StorageProviderActivatedDomainEvent(Id, Code, activatedBy));
    }

    public void Inactivate(Guid? inactivatedBy = null)
    {
        IsActive = false;

        MarkAsUpdated(DateTime.UtcNow, inactivatedBy?.ToString());

        AddDomainEvent(new StorageProviderInactivatedDomainEvent(Id, Code, inactivatedBy));
    }
}
