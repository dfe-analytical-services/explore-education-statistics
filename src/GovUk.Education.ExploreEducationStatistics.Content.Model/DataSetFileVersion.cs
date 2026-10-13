#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Comparers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

/// <summary>
/// A version of a data set file, combining the data and metadata CSVs which are otherwise separate Data and Metadata
/// <see cref="File"/> rows.
/// </summary>
/// <remarks>
/// TODO EES-5105 While <see cref="Database.DataSetFileDualWriteInterceptor"/> is in place, this is a mirror of a
/// Data <see cref="File"/> (sharing its Id) and its Metadata sibling (whose Id is <see cref="MetaFileId"/>).
/// </remarks>
public class DataSetFileVersion
{
    public Guid Id { get; set; }

    public Guid DataSetFileId { get; set; }

    public DataSetFile DataSetFile { get; set; } = null!;

    public int Version { get; set; }

    public Guid SubjectId { get; set; }

    public Guid RootPath { get; set; }

    public string Filename { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long ContentLength { get; set; }

    public Guid? MetaFileId { get; set; }

    public string? MetaFilename { get; set; }

    public string? MetaContentType { get; set; }

    public long? MetaContentLength { get; set; }

    public DataSetFileMeta? DataSetFileMeta { get; set; }

    public List<DataSetFileFilterHierarchy>? FilterHierarchies { get; set; }

    public Guid? ReplacedById { get; set; }

    public DataSetFileVersion? ReplacedBy { get; set; }

    public Guid? ReplacingId { get; set; }

    public DataSetFileVersion? Replacing { get; set; }

    // TODO EES-5105 Implement ICreatedTimestamp once DataSetFileDualWriteInterceptor is removed.
    // Until then, Created is copied from the data File so must not be stamped by DbContextUtils.
    public DateTime? Created { get; set; }

    public Guid? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    internal class Config : IEntityTypeConfiguration<DataSetFileVersion>
    {
        public void Configure(EntityTypeBuilder<DataSetFileVersion> builder)
        {
            builder
                .HasOne(dsfv => dsfv.DataSetFile)
                .WithMany(dsf => dsf.Versions)
                .HasForeignKey(dsfv => dsfv.DataSetFileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(dsfv => new { dsfv.DataSetFileId, dsfv.Version });

            builder.HasIndex(dsfv => dsfv.SubjectId);

            builder.HasIndex(dsfv => dsfv.MetaFileId).IsUnique();

            builder.Property(dsfv => dsfv.ContentType).HasMaxLength(255);

            builder.Property(dsfv => dsfv.MetaContentType).HasMaxLength(255);

            builder
                .Property(dsfv => dsfv.DataSetFileMeta)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<DataSetFileMeta>(v),
                    new JsonValueComparer<DataSetFileMeta>()
                );

            builder
                .Property(dsfv => dsfv.FilterHierarchies)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<DataSetFileFilterHierarchy>>(v),
                    new JsonValueComparer<List<DataSetFileFilterHierarchy>>()
                );

            builder
                .HasOne(dsfv => dsfv.Replacing)
                .WithOne()
                .HasForeignKey<DataSetFileVersion>(dsfv => dsfv.ReplacingId)
                .IsRequired(false);

            builder
                .HasOne(dsfv => dsfv.ReplacedBy)
                .WithOne()
                .HasForeignKey<DataSetFileVersion>(dsfv => dsfv.ReplacedById)
                .IsRequired(false);

            builder
                .Property(dsfv => dsfv.Created)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
        }
    }
}
