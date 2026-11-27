#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IParquetDataSetService
{
    /// <summary>
    /// Creates parquet files for a data file that has already been imported into StatsDB, and marks the data
    /// file as having parquet files. The data file's <see cref="DataStorageVersion" /> is left unchanged.
    /// </summary>
    Task ExportStatsDbDataSet(Guid fileId, CancellationToken cancellationToken = default);
}
