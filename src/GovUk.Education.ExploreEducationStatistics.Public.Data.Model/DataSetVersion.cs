using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersion : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public required Guid DataSetId { get; set; }

    public DataSet DataSet { get; set; } = null!;

    public required DataSetVersionStatus Status { get; set; }

    public required Guid CsvFileId { get; set; }

    public required string ParquetFilename { get; set; }

    public required int VersionMajor { get; set; }

    public required int VersionMinor { get; set; }

    public required string Notes { get; set; }

    public required DataSetVersionMetaSummary MetaSummary { get; set; }

    public DataSetMeta Meta { get; set; } = null!;

    public long TotalResults { get; set; }

    public List<ChangeSetFilters> FilterChanges { get; set; } = [];

    public List<ChangeSetFilterOptions> FilterOptionChanges { get; set; } = [];

    public List<ChangeSetIndicators> IndicatorChanges { get; set; } = [];

    public List<ChangeSetLocations> LocationChanges { get; set; } = [];

    public List<ChangeSetTimePeriods> TimePeriodChanges { get; set; } = [];

    public DateTimeOffset? Published { get; set; }

    public DateTimeOffset? Unpublished { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<DataSetVersion>
    {
        public void Configure(EntityTypeBuilder<DataSetVersion> builder)
        {
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
            });

            builder.Property(dsv => dsv.Status).HasConversion<string>();
        }
    }
}
