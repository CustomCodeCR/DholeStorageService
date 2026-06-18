using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Storage.Domain.Providers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Storage.Persistence.Configurations.Providers;

internal sealed class StorageProviderConfiguration
    : EntityTypeConfigurationBase<StorageProvider, Guid>
{
    public override void Configure(EntityTypeBuilder<StorageProvider> builder)
    {
        base.Configure(builder);

        builder.ToTable("StorageProviders");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(150);
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.UpdatedBy).HasMaxLength(150);

        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.DeletedAtUtc);
        builder.Property(x => x.DeletedBy).HasMaxLength(150);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.Property(x => x.ProviderType).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Configuration).HasColumnType("jsonb");

        builder.Property(x => x.IsDefault).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
    }
}
