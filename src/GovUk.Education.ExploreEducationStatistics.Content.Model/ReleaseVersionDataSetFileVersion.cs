#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Comparers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

/// <summary>
/// Links a <see cref="DataSetFileVersion"/> to a <see cref="ReleaseVersion"/>.
/// </summary>
/// <remarks>
/// TODO EES-5105 While <see cref="Database.DataSetFileDualWriteInterceptor"/> is in place, this is a mirror of the
/// <see cref="ReleaseFile"/> of a Data <see cref="File"/>, sharing its Id.
/// </remarks>
public class ReleaseVersionDataSetFileVersion
{
    public Guid Id { get; set; }

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public Guid DataSetFileVersionId { get; set; }

    public DataSetFileVersion DataSetFileVersion { get; set; } = null!;

    public string? Name { get; set; }

    public string? Summary { get; set; }

    public int Order { get; set; }

    public bool? PublicApiCompatible { get; set; }

    public Guid? PublicApiDataSetId { get; set; }

    public SemVersion? PublicApiDataSetVersion { get; set; }

    public List<FilterSequenceEntry>? FilterSequence { get; set; }

    public List<IndicatorGroupSequenceEntry>? IndicatorSequence { get; set; }

    public DateTime? Published { get; set; }

    internal class Config : IEntityTypeConfiguration<ReleaseVersionDataSetFileVersion>
    {
        public void Configure(EntityTypeBuilder<ReleaseVersionDataSetFileVersion> builder)
        {
            builder.HasQueryFilter(rvdsfv => !rvdsfv.ReleaseVersion.SoftDeleted);

            builder
                .HasOne(rvdsfv => rvdsfv.ReleaseVersion)
                .WithMany()
                .HasForeignKey(rvdsfv => rvdsfv.ReleaseVersionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(rvdsfv => rvdsfv.DataSetFileVersion)
                .WithMany()
                .HasForeignKey(rvdsfv => rvdsfv.DataSetFileVersionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(rvdsfv => rvdsfv.FilterSequence)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<FilterSequenceEntry>>(v),
                    new JsonValueComparer<List<FilterSequenceEntry>>()
                );

            builder
                .Property(rvdsfv => rvdsfv.IndicatorSequence)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<IndicatorGroupSequenceEntry>>(v),
                    new JsonValueComparer<List<IndicatorGroupSequenceEntry>>()
                );

            builder
                .Property(rvdsfv => rvdsfv.Published)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            builder
                .Property(rvdsfv => rvdsfv.PublicApiDataSetVersion)
                .HasMaxLength(20)
                .HasConversion(v => v!.ToString(), v => SemVersion.Parse(v, SemVersionStyles.Strict, 20));

            builder.HasIndex(rvdsfv => new { rvdsfv.ReleaseVersionId, rvdsfv.DataSetFileVersionId }).IsUnique();

            builder
                .HasIndex(rvdsfv => new
                {
                    rvdsfv.ReleaseVersionId,
                    rvdsfv.PublicApiDataSetId,
                    rvdsfv.PublicApiDataSetVersion,
                })
                .IsUnique();
        }
    }
}
