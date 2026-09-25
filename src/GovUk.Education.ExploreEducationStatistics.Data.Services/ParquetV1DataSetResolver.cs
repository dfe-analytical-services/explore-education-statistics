#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

/// <summary>
/// Resolves a data set to a <see cref="ParquetV1DataSet" /> where its data file has a Parquet copy, and to a
/// <see cref="StatisticsDbDataSet" /> otherwise.
/// </summary>
public class ParquetV1DataSetResolver(
    ContentDbContext contentDbContext,
    StatisticsDbDataSetResolver statisticsDbDataSetResolver,
    IDataFilesPathResolver dataFilesPathResolver,
    ILogger<ParquetV1DataSet> logger
) : IStorageDataSetResolver
{
    public async Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var statisticsDbDataSet = await statisticsDbDataSetResolver.Resolve(subjectId, cancellationToken);

        var dataFile = await contentDbContext.Files.SingleOrDefaultAsync(
            file => file.SubjectId == subjectId && file.Type == FileType.Data,
            cancellationToken
        );

        if (dataFile?.HasParquet != true)
        {
            return statisticsDbDataSet;
        }

        return new ParquetV1DataSet(
            dataFile: dataFile,
            statisticsDbDataSet: statisticsDbDataSet,
            dataFilesPathResolver: dataFilesPathResolver,
            logger: logger
        );
    }
}
