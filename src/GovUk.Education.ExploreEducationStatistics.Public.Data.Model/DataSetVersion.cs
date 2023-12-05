using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersion : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public Guid DataSetId { get; set; }

    public DataSet DataSet { get; set; } = null!;

    public Guid CsvFileId { get; set; }

    public string ParquetFilename { get; set; } = string.Empty;

    public int VersionMajor { get; set; }

    public int VersionMinor { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DataSetVersionMetaSummary MetaSummary { get; set; } = null!;

    public List<DataSetChangeFilter> FilterChanges { get; set; } = new();

    public List<DataSetChangeFilterOption> FilterOptionChanges { get; set; } = new();

    public List<DataSetChangeIndicator> IndicatorChanges { get; set; } = new();

    public List<DataSetChangeLocation> LocationChanges { get; set; } = new();

    public List<DataSetChangeTimePeriod> TimePeriodChanges { get; set; } = new();

    public DateTimeOffset? Published { get; set; }

    public DataSetVersionStatus Status { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }
}
