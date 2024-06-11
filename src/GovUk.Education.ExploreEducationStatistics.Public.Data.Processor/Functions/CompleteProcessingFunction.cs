using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CompleteProcessingFunction(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    public async Task CompleteProcessing(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.Completing, cancellationToken);

        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        // Delete the DuckDb database file as it is no longer needed
        File.Delete(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));

        dataSetVersion.Status = dataSetVersion.IsFirstVersion
            ? DataSetVersionStatus.Draft
            : DataSetVersionStatus.Mapping;

        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
