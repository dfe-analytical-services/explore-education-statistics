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

    public required DataSetVersionStatus Status { get; set; }

    public required Guid ReleaseFileId { get; set; }

    public required int VersionMajor { get; set; }

    public required int VersionMinor { get; set; }

    // Not using this currently, but it's being considered for
    // data replacement version numbers in future functionality.
    // We're including this now so that the column exists and is
    // in the right position. Remove if not needed in the end.
    public int VersionPatch { get; set; }

    public required string Notes { get; set; }

    public long TotalResults { get; set; }

    public DataSetVersionMetaSummary? MetaSummary { get; set; }

    public GeographicLevelMeta? GeographicLevelMeta { get; set; }

    public List<LocationMeta> LocationMetas { get; set; } = [];

    public List<FilterMeta> FilterMetas { get; set; } = [];

    public List<IndicatorMeta> IndicatorMetas { get; set; } = [];

    public List<TimePeriodMeta> TimePeriodMetas { get; set; } = [];

    public List<ChangeSetFilters> FilterChanges { get; set; } = [];

    public List<ChangeSetFilterOptions> FilterOptionChanges { get; set; } = [];

    public List<ChangeSetIndicators> IndicatorChanges { get; set; } = [];

    public List<ChangeSetLocations> LocationChanges { get; set; } = [];

    public List<ChangeSetTimePeriods> TimePeriodChanges { get; set; } = [];

    public List<DataSetVersionImport> Imports { get; set; } = [];

    public DateTimeOffset? Published { get; set; }

    public DateTimeOffset? Withdrawn { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public string Version => $"{VersionMajor}.{VersionMinor}";

    public bool IsFirstVersion => Version == "1.0";

    public SemVersion FullSemanticVersion() => new(major: VersionMajor, minor: VersionMinor, patch: VersionPatch);

    public DataSetVersionType VersionType
        => VersionMinor == 0 ? DataSetVersionType.Major : DataSetVersionType.Minor;

    public bool CanBeDeleted => Status is DataSetVersionStatus.Failed
        or DataSetVersionStatus.Mapping
        or DataSetVersionStatus.Draft
        or DataSetVersionStatus.Cancelled;

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

            builder.HasIndex(dsv => new { dsv.DataSetId, dsv.VersionMajor, dsv.VersionMinor, dsv.VersionPatch })
                .HasDatabaseName("IX_DataSetVersions_DataSetId_VersionNumber")
                .IsUnique();

            builder.HasIndex(dsv => dsv.ReleaseFileId);
        }
    }
}
