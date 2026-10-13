#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage;

public class StorageDataSetWriterResolver(StatisticsDbDataSetWriterFactory statisticsDbDataSetWriterFactory)
    : IStorageDataSetWriterResolver
{
    public IStorageDataSetWriter Resolve(File dataFile)
    {
        if (dataFile.Type != FileType.Data || dataFile.SubjectId is null)
        {
            throw new ArgumentException($"File {dataFile.Id} is not a data file linked to a subject", nameof(dataFile));
        }

        var subjectId = dataFile.SubjectId.Value;

        return dataFile.DataStorageVersion switch
        {
            DataStorageVersion.StatsDB => statisticsDbDataSetWriterFactory.Create(subjectId),
            null => throw new InvalidOperationException(
                $"No data storage version recorded for data file {dataFile.Id}"
            ),
            _ => throw new NotSupportedException(
                $"Data storage version {dataFile.DataStorageVersion} is not supported"
            ),
        };
    }
}
