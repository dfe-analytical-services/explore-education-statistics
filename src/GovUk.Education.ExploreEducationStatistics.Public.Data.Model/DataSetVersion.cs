using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersion : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public Guid DataSetId { get; set; }

    public DataSet DataSet { get; set; } = null!;

    public required Guid CsvFileId { get; set; }

    public required string ParquetFilename { get; set; } = string.Empty;

    public required int VersionMajor { get; set; }

    public required int VersionMinor { get; set; }

    public required string Notes { get; set; } = string.Empty;

    public DataSetVersionMetaSummary MetaSummary { get; set; } = null!;

    public DataSetMeta Meta { get; set; } = null!;

    public List<ChangeSetFilters> FilterChanges { get; set; } = new();

    public List<ChangeSetFilterOptions> FilterOptionChanges { get; set; } = new();

    public List<ChangeSetIndicators> IndicatorChanges { get; set; } = new();

    public List<ChangeSetLocations> LocationChanges { get; set; } = new();

    public List<ChangeSetTimePeriods> TimePeriodChanges { get; set; } = new();

    public DateTimeOffset? Published { get; set; }

    public DataSetVersionStatus Status { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }
}
