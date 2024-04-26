using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class DataSetService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext
) : IDataSetService
{
    public async Task<Guid> CreateDataSetVersion(Guid releaseFileId,
        Guid? dataSetId = null,
        CancellationToken cancellationToken = default)
    {
        var dataSet = dataSetId.HasValue
            ? await publicDataDbContext.DataSets
                .Include(ds => ds.LatestLiveVersion)
                .FirstAsync(ds => ds.Id == dataSetId, cancellationToken: cancellationToken)
            : await CreateDataSet(releaseFileId, cancellationToken);

        var file = await contentDbContext.ReleaseFiles
            .Where(rf => rf.Id == releaseFileId)
            .Select(rf => rf.File)
            .FirstAsync(cancellationToken: cancellationToken);

        var dataSetVersion = new DataSetVersion
        {
            ReleaseFileId = releaseFileId,
            DataSetId = dataSet.Id,
            Status = DataSetVersionStatus.Processing,
            Imports =
            [
                new DataSetVersionImport
                {
                    InstanceId = Guid.NewGuid(), Stage = DataSetVersionImportStage.Created
                }
            ],
            Notes = "",
            VersionMajor = dataSet.LatestLiveVersion?.VersionMajor ?? 1,
            VersionMinor = dataSet.LatestLiveVersion?.VersionMinor + 1 ?? 0
        };

        dataSet.Versions.Add(dataSetVersion);
        dataSet.LatestDraftVersion = dataSetVersion;

        // TODO does this work if a dataset id is passed in?
        // publicDataDbContext.DataSets.Update(dataSet);
        // await publicDataDbContext.SaveChangesAsync(cancellationToken);

        file.PublicDataSetVersionId = dataSetVersion.Id;
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return dataSetVersion.Id;
    }

    private async Task<DataSet> CreateDataSet(Guid releaseFileId, CancellationToken cancellationToken)
    {
        var file = await contentDbContext.ReleaseFiles
            .Where(rf => rf.Id == releaseFileId)
            .Select(rf => new
            {
                Title = rf.Name!, rf.Summary, rf.ReleaseVersion.PublicationId
            })
            .FirstAsync(cancellationToken: cancellationToken);

        var dataSet = new DataSet
        {
            Status = DataSetStatus.Draft,
            Title = file.Title,
            Summary = file.Summary ?? "",
            PublicationId = file.PublicationId,
        };

        return dataSet;
    }
}
