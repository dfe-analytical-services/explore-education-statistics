using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersion : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public required Guid DataSetId { get; set; }

    public DataSet DataSet { get; set; } = null!;

    public required Release Release { get; set; }

    public required DataSetVersionStatus Status { get; set; }

    public required int VersionMajor { get; set; }

    public required int VersionMinor { get; set; }

    public int VersionPatch { get; set; }

    public required string Notes { get; set; }

    public long TotalResults { get; set; }

    public DataSetVersionMetaSummary? MetaSummary { get; set; }

    public GeographicLevelMeta? GeographicLevelMeta { get; set; }

    public List<LocationMeta> LocationMetas { get; set; } = [];

    public List<FilterMeta> FilterMetas { get; set; } = [];

    public List<IndicatorMeta> IndicatorMetas { get; set; } = [];

    public List<TimePeriodMeta> TimePeriodMetas { get; set; } = [];

    public List<FilterMetaChange> FilterMetaChanges { get; set; } = [];

    public List<FilterOptionMetaChange> FilterOptionMetaChanges { get; set; } = [];

    public GeographicLevelMetaChange? GeographicLevelMetaChange { get; set; }

    public List<IndicatorMetaChange> IndicatorMetaChanges { get; set; } = [];

    public List<LocationMetaChange> LocationMetaChanges { get; set; } = [];

    public List<LocationOptionMetaChange> LocationOptionMetaChanges { get; set; } = [];

    public List<TimePeriodMetaChange> TimePeriodMetaChanges { get; set; } = [];

    public List<DataSetVersionImport> Imports { get; set; } = [];

    public List<PreviewToken> PreviewTokens { get; set; } = [];

    public DateTimeOffset? Published { get; set; }

    public DateTimeOffset? Withdrawn { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public string PublicVersion => VersionPatch > 0 
        ? $"{VersionMajor}.{VersionMinor}.{VersionPatch}" 
        : $"{VersionMajor}.{VersionMinor}";

    public SemVersion SemVersion() => new(major: VersionMajor, minor: VersionMinor, patch: VersionPatch);

    public SemVersion DefaultNextVersion() => SemVersion().WithMinor(VersionMinor + 1).WithPatch(0);
    
    public SemVersion NextPatchVersion() => SemVersion().WithPatch(VersionPatch + 1);

    public bool IsFirstVersion => VersionMajor == 1 && VersionMinor == 0 && VersionPatch == 0;

    public DataSetVersionType VersionType
        => VersionPatch == 0 
            ? VersionMinor == 0 
                ? DataSetVersionType.Major 
                : DataSetVersionType.Minor 
            : DataSetVersionType.Patch;

    public bool CanBeDeleted => Status is DataSetVersionStatus.Failed
        or DataSetVersionStatus.Finalising
        or DataSetVersionStatus.Mapping
        or DataSetVersionStatus.Draft
        or DataSetVersionStatus.Cancelled;

    public bool CanBeUpdated => Status is DataSetVersionStatus.Mapping 
        or DataSetVersionStatus.Finalising
        or DataSetVersionStatus.Draft;

    internal class Config : IEntityTypeConfiguration<DataSetVersion>
    {
        public void Configure(EntityTypeBuilder<DataSetVersion> builder)
        {
            builder.Property(dsv => dsv.Id)
                .HasValueGenerator<UuidV7ValueGenerator>();

            builder.Property(dsv => dsv.Status)
                .HasConversion<string>();

            builder.OwnsOne(v => v.MetaSummary, ms =>
            {
                ms.ToJson();
                ms.OwnsOne(msb => msb.TimePeriodRange, msb =>
                {
                    msb.OwnsOne(tpr => tpr.Start, tpr =>
                    {
                        tpr.Property(tpm => tpm.Code)
                            .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>());
                    });
                    msb.OwnsOne(tpr => tpr.End, tpr =>
                    {
                        tpr.Property(tpm => tpm.Code)
                            .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>());
                    });
                });

                ms.Property(msb => msb.GeographicLevels)
                    .HasColumnType("text[]")
                    .HasConversion(
                        value => value
                            .Select(EnumToEnumValueConverter<GeographicLevel>.ToProvider)
                            .ToList(),
                        value => value
                            .Select(EnumToEnumValueConverter<GeographicLevel>.FromProvider)
                            .ToList(),
                        new ValueComparer<List<GeographicLevel>>(
                            (c1, c2) => c1!.SequenceEqual(c2!),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList())
                    );
            });

            builder.OwnsOne(dsv => dsv.Release,
                ownedBuilder =>
                {
                    ownedBuilder
                        .HasIndex(r => r.DataSetFileId);
                    ownedBuilder
                        .HasIndex(r => r.ReleaseFileId)
                        .IsUnique();
                });

            builder.HasIndex(dsv => new
                {
                    dsv.DataSetId,
                    dsv.VersionMajor,
                    dsv.VersionMinor,
                    dsv.VersionPatch
                })
                .HasDatabaseName("IX_DataSetVersions_DataSetId_VersionNumber")
                .IsUnique();
        }
    }
}
