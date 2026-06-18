using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Storage.Domain.Files.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Storage.Persistence.Configurations.Files;

internal sealed class StorageFileConfiguration : EntityTypeConfigurationBase<StorageFile, Guid>
{
    public override void Configure(EntityTypeBuilder<StorageFile> builder)
    {
        base.Configure(builder);

        builder.ToTable("Files");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(150);
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.UpdatedBy).HasMaxLength(150);

        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.DeletedAtUtc);
        builder.Property(x => x.DeletedBy).HasMaxLength(150);

        builder.Property(x => x.ProviderId).IsRequired();

        builder.Property(x => x.OriginalFileName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(300).IsRequired();

        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Extension).HasMaxLength(50);

        builder.Property(x => x.SizeInBytes).IsRequired();

        builder.Property(x => x.StoragePath).HasColumnType("text").IsRequired();

        builder.Property(x => x.Checksum).HasMaxLength(250);

        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();

        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");

        builder.Property(x => x.CurrentVersionNumber).IsRequired();

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsDeleted);

        builder
            .HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.Versions)
            .HasField("_versions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(x => x.References)
            .WithOne()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.References)
            .HasField("_references")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
