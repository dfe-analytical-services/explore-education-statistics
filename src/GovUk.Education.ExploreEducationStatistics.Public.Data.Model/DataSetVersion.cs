using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersion : ICreatedUpdatedTimestamps<DateTime, DateTime>
{
    public Guid Id { get; init; }
    public Guid DataSetId { get; set; }
    public DataSet DataSet { get; set; } = null!;
    public Guid CsvFileId { get; set; }
    public string ParquetFilename { get; set; } = string.Empty;
    public int VersionMajor { get; set; }
    public int VersionMinor { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<DataSetVersionChange> Changes { get; set; } = new();
    public DateTime Published { get; set; }
    public DataSetVersionStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
