using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Storage.Domain.Providers.Enums;
using Dhole.Storage.Domain.Providers.Events;

namespace Dhole.Storage.Domain.Providers.Entities;

public sealed class Provider : SoftDeletableAggregateRoot<Guid>
{
    private Provider() { }

    private Provider(
        Guid id,
        string code,
        string name,
        ProviderType providerType,
        string? configuration,
        bool isDefault,
        Guid? createdBy
    )
        : base(id)
    {
        Code = code.Trim();
        Name = name.Trim();
        ProviderType = providerType;
        Configuration = configuration;
        IsDefault = isDefault;
        IsActive = true;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ProviderType ProviderType { get; private set; }
    public string? Configuration { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public static Provider Create(
        string code,
        string name,
        ProviderType providerType,
        string? configuration,
        bool isDefault,
        Guid? createdBy
    )
    {
        var provider = new Provider(
            Guid.NewGuid(),
            code,
            name,
            providerType,
            configuration,
            isDefault,
            createdBy
        );

        provider.AddDomainEvent(
            new ProviderCreatedDomainEvent(
                provider.Id,
                provider.Code,
                provider.Name,
                provider.ProviderType.ToString(),
                createdBy
            )
        );

        return provider;
    }

    public void Update(string name, string? configuration, bool isDefault, Guid? updatedBy)
    {
        Name = name.Trim();
        Configuration = configuration;
        IsDefault = isDefault;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(
            new ProviderUpdatedDomainEvent(Id, Code, Name, ProviderType.ToString(), updatedBy)
        );
    }

    public void SetActive(bool isActive, Guid? updatedBy)
    {
        if (IsActive == isActive)
            return;

        IsActive = isActive;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        if (IsActive)
        {
            AddDomainEvent(new ProviderActivatedDomainEvent(Id, Code, updatedBy));
            return;
        }

        AddDomainEvent(new ProviderInactivatedDomainEvent(Id, Code, updatedBy));
    }
}
