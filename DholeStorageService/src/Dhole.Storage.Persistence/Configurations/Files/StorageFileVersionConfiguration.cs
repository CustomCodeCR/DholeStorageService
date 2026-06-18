using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Storage.Domain.Files.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Storage.Persistence.Configurations.Files;

internal sealed class StorageFileVersionConfiguration
    : EntityTypeConfigurationBase<StorageFileVersion, Guid>
{
    public override void Configure(EntityTypeBuilder<StorageFileVersion> builder)
    {
        base.Configure(builder);

        builder.ToTable("FileVersions");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(150);
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.UpdatedBy).HasMaxLength(150);

        builder.Property(x => x.FileId).IsRequired();

        builder.Property(x => x.VersionNumber).IsRequired();

        builder.Property(x => x.StoredFileName).HasMaxLength(300).IsRequired();

        builder.Property(x => x.StoragePath).HasColumnType("text").IsRequired();

        builder.Property(x => x.SizeInBytes).IsRequired();

        builder.Property(x => x.Checksum).HasMaxLength(250);

        builder.HasIndex(x => new { x.FileId, x.VersionNumber }).IsUnique();
    }
}
