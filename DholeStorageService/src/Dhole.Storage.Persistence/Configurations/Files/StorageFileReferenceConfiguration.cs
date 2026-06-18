using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Storage.Domain.Files.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Storage.Persistence.Configurations.Files;

internal sealed class StorageFileReferenceConfiguration
    : EntityTypeConfigurationBase<StorageFileReference, Guid>
{
    public override void Configure(EntityTypeBuilder<StorageFileReference> builder)
    {
        base.Configure(builder);

        builder.ToTable("FileReferences");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(150);
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.UpdatedBy).HasMaxLength(150);

        builder.Property(x => x.FileId).IsRequired();

        builder.Property(x => x.SourceService).HasMaxLength(150).IsRequired();

        builder.Property(x => x.EntityType).HasMaxLength(150).IsRequired();

        builder.Property(x => x.EntityId).IsRequired();

        builder.Property(x => x.ReferenceType).HasMaxLength(100);

        builder.HasIndex(x => x.FileId);

        builder.HasIndex(x => new
        {
            x.SourceService,
            x.EntityType,
            x.EntityId,
        });
    }
}
