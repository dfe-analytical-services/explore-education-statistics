using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessCompletionOfNextDataSetVersionFunctions(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IDataSetVersionChangeService dataSetVersionChangeService) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(ActivityNames.CreateChanges)]
    public async Task CreateChanges(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.CreatingChanges, cancellationToken);
        await dataSetVersionChangeService.CreateChanges(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }

    [Function(ActivityNames.UpdateFileStoragePath)]
    public async Task UpdateFileStoragePath(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);

        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        var currentLiveVersion = await publicDataDbContext
            .DataSets
            .Where(dataSet => dataSet.Id == dataSetVersion.DataSetId)
            .Select(dataSet => dataSet.LatestLiveVersion!)
            .SingleAsync(cancellationToken);

        var originalPath = dataSetVersionPathResolver.DirectoryPath(
            dataSetVersion: dataSetVersion,
            versionNumber: currentLiveVersion.DefaultNextVersion());

        var newPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        if (originalPath != newPath)
        {
            Directory.Move(sourceDirName: originalPath, destDirName: newPath);
        }
    }

    [Function(ActivityNames.CompleteNextDataSetVersionImportProcessing)]
    public async Task CompleteNextDataSetVersionImportProcessing(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.Completing, cancellationToken);

        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        // Delete the DuckDb database file as it is no longer needed
        File.Delete(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));

        dataSetVersion.Status = DataSetVersionStatus.Draft;

        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
